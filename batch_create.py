#!/usr/bin/env python3
"""
Batch create topic branches for BovineLabs Core Internals.
Uses git plumbing to create branches without checking out.
Reads source code from com.bovinelabs.core, generates README.md, commits.
"""

import json
import os
import re
import subprocess
import sys

REPO = "/home/l/Github/bovinelabs-core-internals"
SRC = "/home/l/Github/com.bovinelabs.core"
GITLAB = "https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master"


def git(args, **kw):
    r = subprocess.run(["git"] + args, capture_output=True, text=True, cwd=REPO, **kw)
    return r


def read_source(rel_path):
    full = os.path.join(SRC, rel_path)
    if not os.path.exists(full):
        return None
    with open(full) as f:
        return f.read()


def extract_struct_fields(source):
    """Extract fields from the first struct/class found in source."""
    fields = []
    methods = []
    
    for m in re.finditer(r'^\s+(?:public|internal)\s+(?!class|struct|interface|enum|override|virtual|abstract|static\s+class|const\s+string)([\w<>\[\]\*?,\s]+?)\s+(\w+)\s*(?:=|;|{)', source, re.MULTILINE):
        typ = m.group(1).strip().split()[-1]  # get the type name
        name = m.group(2).strip()
        if name not in ('get', 'set', 'if', 'for', 'while', 'new', 'return', 'void', 'class', 'struct'):
            fields.append((typ[:22], name))
    
    for m in re.finditer(r'^\s+public\s+(?:static\s+|override\s+|virtual\s+|unsafe\s+)*(?:readonly\s+)?([\w<>\[\]\*?]+)\s+(\w+)\s*\(([^)]{0,80})\)', source, re.MULTILINE):
        ret = m.group(1).strip()
        name = m.group(2).strip()
        args = m.group(3).strip()[:50]
        if ret and name and name[0].isupper():
            methods.append((ret[:15], name, args))
    
    return fields, methods


def generate_readme(topic, source, rel_path):
    """Generate complete README.md content from source code."""
    W = 72
    parts = topic.replace(".", " ").split()
    class_name = parts[-1] if parts else topic
    
    fields, methods = extract_struct_fields(source) if source else ([], [])
    
    lines = []
    lines.append(f"# {topic}")
    lines.append("")
    lines.append("## Inner Workings Diagram")
    lines.append("")
    
    # Overview
    lines.append("```")
    lines.append(f" {topic}")
    lines.append(" " + "=" * (W - 2))
    
    if source:
        # Find class/struct definition
        type_match = re.search(r'(?:public|internal)\s+(?:sealed\s+|abstract\s+|partial\s+)*(?:struct|class|interface)\s+(\w+)', source)
        if type_match:
            type_name = type_match.group(1)
            lines.append(f" Defined as: {type_name}")
        
        # Find namespace
        ns_match = re.search(r'namespace\s+([\w.]+)', source)
        if ns_match:
            lines.append(f" Namespace:  {ns_match.group(1)}")
        
        lines.append("")
    
    # Show struct layout if we found fields
    if fields:
        lines.append(" Structure:")
        lines.append(" ┌" + "─" * (W - 4) + "┐")
        
        type_name = ""
        if source:
            tm = re.search(r'(?:public|internal)\s+(?:sealed\s+|abstract\s+|partial\s+)*(?:struct|class)\s+(\w+)', source)
            if tm:
                type_name = tm.group(1)
        
        if type_name:
            hdr = f" │ {type_name}"
            hdr += " " * (W - 4 - len(hdr) + 2) + "│"
            lines.append(hdr[:W])
            lines.append(" ├" + "─" * (W - 4) + "┤")
        
        for typ, name in fields[:15]:
            line = f" │ {typ:<24} {name}"
            pad = W - 4 - len(line) + 2
            if pad > 0:
                line += " " * pad
            line += "│"
            lines.append(line[:W])
        
        lines.append(" └" + "─" * (W - 4) + "┘")
    
    # Show methods if we found them
    if methods:
        lines.append("")
        lines.append(" Key Methods:")
        lines.append(" ┌" + "─" * (W - 4) + "┐")
        
        for ret, name, args in methods[:10]:
            line = f" │ {name}({args})"
            pad = W - 4 - len(line) + 2
            if pad > 0:
                line += " " * pad
            line += "│"
            lines.append(line[:W])
            
            ret_line = f" │   → {ret}"
            pad = W - 4 - len(ret_line) + 2
            if pad > 0:
                ret_line += " " * pad
            ret_line += "│"
            lines.append(ret_line[:W])
        
        lines.append(" └" + "─" * (W - 4) + "┘")
    
    lines.append("```")
    lines.append("")
    
    # Source links
    lines.append("## Source")
    lines.append("")
    lines.append(f"- [{rel_path}]({GITLAB}/{rel_path})")
    lines.append("")
    
    return "\n".join(lines)


def create_topic_branch(topic, rel_path):
    """Create a topic branch with README.md using git plumbing."""
    branch = "topic/" + topic.replace(".", "_")
    
    # Check if branch exists
    r = git(["rev-parse", "--verify", branch])
    if r.returncode == 0:
        return "SKIP", "already exists"
    
    source = read_source(rel_path)
    if source is None:
        # Still create with minimal content
        source = None
    
    readme = generate_readme(topic, source, rel_path)
    
    # Create the branch from main
    git(["checkout", "main"])
    r = git(["checkout", "-b", branch])
    if r.returncode != 0:
        return "ERROR", f"checkout -b failed: {r.stderr}"
    
    # Write README
    readme_path = os.path.join(REPO, "README.md")
    with open(readme_path, "w") as f:
        f.write(readme)
    
    git(["add", "README.md"])
    r = git(["commit", "-m", f"{branch}: ASCII inner workings diagram"])
    if r.returncode != 0:
        return "ERROR", f"commit failed: {r.stderr}"
    
    return "OK", branch


def main():
    batch_num = int(sys.argv[1]) if len(sys.argv) > 1 else 1
    batch_size = 25
    
    with open(os.path.join(REPO, "topic_sources.json")) as f:
        topic_sources = json.load(f)
    
    # Get existing branches
    r = git(["branch"])
    branches = set(b.strip().lstrip("* ") for b in r.stdout.strip().split("\n") 
                   if b.strip().startswith("topic/"))
    
    todos = [t for t in topic_sources.keys() 
             if "topic/" + t.replace(".", "_") not in branches]
    
    start = (batch_num - 1) * batch_size
    end = start + batch_size
    batch = todos[start:end]
    
    if not batch:
        print(f"Batch {batch_num}: No topics to process (all done!)")
        sys.exit(0)
    
    print(f"Batch {batch_num}: {len(batch)} topics ({start+1}-{start+len(batch)} of {len(todos)} remaining)")
    print("─" * 60)
    
    # Ensure we start on main
    git(["checkout", "main"])
    
    ok = skip = err = 0
    for i, topic in enumerate(batch, 1):
        rel_path = topic_sources.get(topic)
        if not rel_path:
            print(f"  [{i:2d}/{len(batch)}] o {topic} (no source)")
            skip += 1
            continue
        
        status, msg = create_topic_branch(topic, rel_path)
        git(["checkout", "main"])  # return to main
        
        sym = {"OK": "+", "SKIP": "o", "ERROR": "X"}[status]
        print(f"  [{i:2d}/{len(batch)}] {sym} {topic}")
        
        if status == "OK":
            ok += 1
        elif status == "SKIP":
            skip += 1
        else:
            err += 1
            print(f"           ERROR: {msg}")
    
    print(f"\nBatch {batch_num}: {ok} created, {skip} skipped, {err} errors")
    print(f"Remaining: {len(todos) - start - ok} topics left")


if __name__ == "__main__":
    main()
