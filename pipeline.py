#!/usr/bin/env python3
"""
Parallel pipeline: Create topic branches + Gemini fact-check.
Uses Gemini 2.5 Pro (or flash as fallback) to validate ASCII diagrams.
"""

import json
import os
import re
import subprocess
import sys
import time

REPO = "/home/l/Github/bovinelabs-core-internals"
SOURCE = "/home/l/Github/com.bovinelabs.core"
GEMINI_MODEL = "gemini-2.5-pro"
GEMINI_FALLBACK = "gemini-2.5-flash"


def run(cmd, timeout=30):
    r = subprocess.run(cmd, shell=True, capture_output=True, text=True, timeout=timeout, cwd=REPO)
    return r.stdout.strip(), r.stderr.strip(), r.returncode


def gemini_factcheck(content, model=GEMINI_MODEL, timeout=90):
    """Send README content to Gemini for fact-checking."""
    prompt = f"""You are an expert Unity ECS (Entities 1.x / DOTS) developer. 
Fact-check this ASCII architecture diagram for accuracy:
1. Is the described API behavior correct?
2. Are memory layouts accurate?
3. Any factual errors?

List ONLY factual errors with brief corrections. If no errors, say exactly: NO ERRORS
Be very concise - max 3 bullet points.

{content}"""
    
    try:
        r = subprocess.run(
            ["gemini", "--model", model, "-p", prompt],
            capture_output=True, text=True, timeout=timeout, cwd=REPO
        )
        output = r.stdout.strip()
        # Filter out credential loading messages
        lines = [l for l in output.split('\n') if 'Loaded cached' not in l and 'logout' not in l and '__HERMES' not in l]
        return '\n'.join(lines).strip()
    except subprocess.TimeoutExpired:
        if model == GEMINI_MODEL:
            print(f"  Pro timed out, trying {GEMINI_FALLBACK}...")
            return gemini_factcheck(content, GEMINI_FALLBACK, timeout=60)
        return "ERROR: Gemini timed out"
    except Exception as e:
        return f"ERROR: {e}"


def get_missing_topics():
    """Parse TODO.md and find topics without branches."""
    with open(os.path.join(REPO, "TODO.md")) as f:
        lines = f.readlines()
    
    out, _, _ = run("git branch | grep 'topic/' | sed 's/^[* ]*//' | sort")
    branches = set(b.replace("topic/", "") for b in out.split('\n') if b.strip())
    
    missing = []
    for line in lines:
        if line.startswith('- [ ] '):
            name = line[6:].strip()
            branch = name.replace('.', '_').replace(' ', '_')
            if branch not in branches:
                missing.append((name, branch))
    return missing


def find_source_file(keyword):
    """Find source file containing keyword."""
    try:
        r = subprocess.run(
            f'find {SOURCE} -name "*.cs" | xargs grep -l "{keyword}" 2>/dev/null | head -5',
            shell=True, capture_output=True, text=True, timeout=30
        )
        files = [f for f in r.stdout.strip().split('\n') if f.strip()]
        return files
    except:
        return []


def read_source(path, max_lines=200):
    """Read source file."""
    try:
        with open(path) as f:
            lines = f.readlines()
        return ''.join(lines[:max_lines])
    except:
        return ""


def factcheck_existing_branch(branch_name):
    """Fact-check an already-created branch."""
    out, _, rc = run(f"git show topic/{branch_name}:README.md")
    if rc != 0:
        return "ERROR: No README.md on branch"
    
    result = gemini_factcheck(out)
    return result


def batch_factcheck(branch_list, delay=3):
    """Fact-check multiple branches with rate limit handling."""
    results = {}
    for branch in branch_list:
        print(f"Fact-checking {branch}...")
        out, _, rc = run(f"git show topic/{branch}:README.md")
        if rc != 0:
            results[branch] = "SKIP: No README.md"
            continue
        results[branch] = gemini_factcheck(out)
        if "NO ERRORS" not in results[branch]:
            print(f"  ISSUES: {results[branch][:100]}")
        else:
            print(f"  OK")
        time.sleep(delay)
    return results


if __name__ == "__main__":
    cmd = sys.argv[1] if len(sys.argv) > 1 else "help"
    
    if cmd == "missing":
        topics = get_missing_topics()
        print(f"Missing topics: {len(topics)}")
        for name, branch in topics:
            print(f"  {name} -> topic/{branch}")
    
    elif cmd == "factcheck-all":
        out, _, _ = run("git branch | grep 'topic/' | sed 's/^[* ]*//' | sort")
        branches = [b.replace("topic/", "") for b in out.split('\n') if b.strip()]
        print(f"Fact-checking {len(branches)} branches with Gemini {GEMINI_MODEL}...")
        results = batch_factcheck(branches)
        
        errors = {k: v for k, v in results.items() if "NO ERRORS" not in v and "SKIP" not in v}
        clean = {k: v for k, v in results.items() if "NO ERRORS" in v}
        print(f"\n{'='*60}")
        print(f"Results: {len(clean)} clean, {len(errors)} with issues")
        for branch, issues in errors.items():
            print(f"\n  [{branch}]")
            for line in issues.split('\n'):
                if line.strip():
                    print(f"    {line}")
    
    elif cmd == "factcheck":
        if len(sys.argv) < 3:
            print("Usage: pipeline.py factcheck <branch_name>")
            sys.exit(1)
        branch = sys.argv[2]
        print(f"Fact-checking {branch}...")
        result = factcheck_existing_branch(branch)
        print(result)
    
    elif cmd == "find":
        if len(sys.argv) < 3:
            print("Usage: pipeline.py find <keyword>")
            sys.exit(1)
        files = find_source_file(sys.argv[2])
        for f in files:
            print(f"  {f}")
    
    else:
        print("Commands: missing, factcheck-all, factcheck <branch>, find <keyword>")
