#!/usr/bin/env python3
"""
Clean all topic branches so they ONLY contain README.md.
Removes TODO.md, *.py, *.txt, *.json from every topic branch.
"""

import subprocess
import sys

REPO = "/home/l/Github/bovinelabs-core-internals"

JUNK_FILES = [
    "TODO.md",
    "diagram_builder.py",
    "factcheck.py",
    "factcheck_results.txt",
    "batch_create.py",
    "topic_sources.json",
    "upgrade_diagrams.py",
]


def git(args, **kwargs):
    return subprocess.run(
        ["git"] + args,
        capture_output=True, text=True, cwd=REPO, **kwargs
    )


def clean_branch(branch):
    """Remove all junk files from a branch, keeping only README.md."""
    # List files on this branch
    r = git(["ls-tree", "--name-only", branch])
    if r.returncode != 0:
        return "ERROR", f"ls-tree failed: {r.stderr.strip()}"
    
    files = r.stdout.strip().split("\n")
    junk = [f for f in files if f in JUNK_FILES]
    
    if not junk:
        return "CLEAN", "already clean"
    
    # Checkout branch
    git(["checkout", branch])
    
    # Remove junk files
    for f in junk:
        git(["rm", "-f", f])
    
    # Commit if there are changes
    r = git(["diff", "--cached", "--quiet"])
    if r.returncode != 0:
        git(["commit", "-m", f"Clean: remove tooling files (README.md only)"])
        return "OK", f"removed {', '.join(junk)}"
    
    return "CLEAN", "no changes needed"


def main():
    batch_num = int(sys.argv[1]) if len(sys.argv) > 1 else 1
    batch_size = 30

    r = git(["branch"])
    branches = [
        b.strip().lstrip("* ")
        for b in r.stdout.strip().split("\n")
        if b.strip().startswith("topic/")
    ]

    start = (batch_num - 1) * batch_size
    batch = branches[start:start + batch_size]

    if not batch:
        print(f"Batch {batch_num}: No branches to process")
        return

    print(f"Batch {batch_num}: {len(batch)} branches ({start+1}-{start+len(batch)} of {len(branches)})")
    print("─" * 60)

    git(["checkout", "main"])

    ok = clean_count = err = 0
    for i, branch in enumerate(batch, 1):
        status, msg = clean_branch(branch)
        git(["checkout", "main"])

        sym = {"OK": "+", "CLEAN": "=", "ERROR": "X"}.get(status, "?")
        print(f"  [{i:2d}/{len(batch)}] {sym} {branch}: {msg}")

        if status == "OK":
            ok += 1
        elif status == "CLEAN":
            clean_count += 1
        else:
            err += 1

    print(f"\nBatch {batch_num}: {ok} cleaned, {clean_count} already clean, {err} errors")


if __name__ == "__main__":
    main()
