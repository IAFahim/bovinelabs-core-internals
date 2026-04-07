#!/usr/bin/env python3
"""Batch fact-check topic branches with Gemini Flash - chunked for reliability."""
import subprocess, time, sys, os

REPO = "/home/l/Github/bovinelabs-core-internals"
MODEL = "gemini-2.5-flash"
TIMEOUT = 45
RESULTS_FILE = os.path.join(REPO, "factcheck_results.txt")

def run_sh(cmd, timeout=60):
    r = subprocess.run(cmd, shell=True, capture_output=True, text=True, timeout=timeout, cwd=REPO)
    return r.stdout.strip(), r.stderr.strip(), r.returncode

def gemini_check(content):
    prompt = f"""You are a Unity ECS (Entities 1.x DOTS) expert. Fact-check this ASCII diagram.
List ONLY factual errors (max 3 bullets). If correct, respond: NO ERRORS

{content[:3000]}"""
    try:
        result = subprocess.run(
            ["gemini", "--model", MODEL, "-p", prompt],
            capture_output=True, text=True, timeout=TIMEOUT, cwd=REPO,
            stdin=subprocess.DEVNULL
        )
        output = result.stdout.strip()
        lines = [l for l in output.split('\n') 
                 if 'Loaded cached' not in l and 'logout' not in l and '__HERMES' not in l
                 and 'tcsetattr' not in l and 'Gaxios' not in l and 'job control' not in l]
        return '\n'.join(lines).strip()
    except subprocess.TimeoutExpired:
        return "ERROR: TIMEOUT"
    except Exception as e:
        return f"ERROR: {e}"

def load_progress():
    """Load previously checked branches."""
    done = {}
    if os.path.exists(RESULTS_FILE):
        with open(RESULTS_FILE) as f:
            current_branch = None
            for line in f:
                line = line.strip()
                if line.startswith("CLEAN: "):
                    done[line[7:]] = "CLEAN"
                    current_branch = None
                elif line.startswith("ISSUES: "):
                    current_branch = line[8:]
                    done[current_branch] = ""
                elif line.startswith("SKIP: "):
                    done[line[6:]] = "SKIP"
                    current_branch = None
                elif current_branch and line:
                    done[current_branch] += line + "\n"
    return done

# Get branches
out, _, _ = run_sh("git branch | grep 'topic/' | sed 's/^[* ]*//' | sort")
branches = [b.strip().replace("topic/", "") for b in out.split('\n') if b.strip()]

# Load previous progress
done = load_progress()
remaining = [b for b in branches if b not in done]

print(f"Total: {len(branches)}, Already checked: {len(done)}, Remaining: {len(remaining)}")

if not remaining:
    print("All branches already checked!")
    sys.exit(0)

clean = [b for b, v in done.items() if v == "CLEAN"]
issues_map = {b: v for b, v in done.items() if v not in ("CLEAN", "SKIP", "")}
skipped = [b for b, v in done.items() if v == "SKIP" or "ERROR" in v]

# Process remaining
for i, branch in enumerate(remaining):
    print(f"[{len(done)+i+1}/{len(branches)}] {branch:<50}", end=" ", flush=True)
    
    out, _, rc = run_sh(f"git show topic/{branch}:README.md 2>/dev/null")
    if rc != 0 or not out.strip():
        print("SKIP")
        skipped.append(branch)
        with open(RESULTS_FILE, 'a') as f:
            f.write(f"SKIP: {branch}\n")
        continue
    
    result = gemini_check(out)
    
    if "NO ERRORS" in result.upper():
        print("OK")
        clean.append(branch)
        with open(RESULTS_FILE, 'a') as f:
            f.write(f"CLEAN: {branch}\n")
    elif "ERROR" in result or "TIMEOUT" in result:
        print(result[:60])
        skipped.append(branch)
        with open(RESULTS_FILE, 'a') as f:
            f.write(f"SKIP: {branch}\n")
    else:
        print("ISSUES")
        issues_map[branch] = result
        with open(RESULTS_FILE, 'a') as f:
            f.write(f"ISSUES: {branch}\n{result}\n\n")
    
    time.sleep(1)

# Final summary
print(f"\n{'='*60}")
print(f"FACT-CHECK COMPLETE ({MODEL})")
print(f"{'='*60}")
print(f"Clean:   {len(clean)}")
print(f"Issues:  {len(issues_map)}")
print(f"Skipped: {len(skipped)}")
print(f"Total:   {len(branches)}")

if issues_map:
    print(f"\nBRANCHES WITH ISSUES:")
    for branch, detail in sorted(issues_map.items()):
        print(f"\n  [{branch}]")
        for line in detail.split('\n'):
            if line.strip():
                print(f"    {line.strip()}")
