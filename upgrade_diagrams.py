#!/usr/bin/env python3
"""
Upgrade basic topic diagrams to detailed versions.
Reads source code deeply, generates flow/memory/comparison diagrams.
"""

import json
import os
import re
import subprocess
import sys
import textwrap

REPO = "/home/l/Github/bovinelabs-core-internals"
SRC = "/home/l/Github/com.bovinelabs.core"
GITLAB = "https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master"
W = 72


def read_source(rel_path):
    full = os.path.join(SRC, rel_path)
    if not os.path.exists(full):
        return None
    with open(full) as f:
        return f.read()


def read_branch_readme(branch):
    r = subprocess.run(["git", "show", f"{branch}:README.md"], capture_output=True, text=True, cwd=REPO)
    return r.stdout if r.returncode == 0 else None


def extract_type_info(source):
    """Deep extraction of types, fields, methods, attributes, inheritance."""
    if not source:
        return {}
    
    info = {
        "namespace": None,
        "types": [],       # [(kind, name, bases, attributes)]
        "fields": [],      # [(visibility, type, name, default, is_const)]
        "methods": [],     # [(visibility, return_type, name, params, is_static, is_override)]
        "enums": [],       # [(name, values)]
        "interfaces": [],  # [name]
        "attributes": [],  # [attribute_text]
        "has_unsafe": False,
        "has_burst": False,
        "line_count": 0,
    }
    
    info["line_count"] = source.count('\n')
    info["has_unsafe"] = "unsafe" in source
    info["has_burst"] = "BurstCompile" in source or "BurstDiscard" in source
    
    # Namespace
    m = re.search(r'namespace\s+([\w.]+)', source)
    if m:
        info["namespace"] = m.group(1)
    
    # Attributes on types
    for m in re.finditer(r'\[(\w+(?:\([^)]*\))?)\]\s*\n\s*(?:public|internal)', source):
        info["attributes"].append(m.group(1))
    
    # Type declarations
    for m in re.finditer(
        r'(?:(\[.*?\])\s*\n\s*)?'  # attributes
        r'(?:public|internal)\s+'
        r'(?:(sealed|abstract|partial|static|unsafe)\s+)*'
        r'(struct|class|interface|enum)\s+'
        r'(\w+)'                     # name
        r'(?:\s*<[^>]+>)?'          # generic params
        r'(?:\s*:\s*([^{]+?))?'     # bases
        r'\s*\{', source, re.MULTILINE | re.DOTALL):
        
        kind = m.group(2)
        name = m.group(3)
        bases = m.group(4).strip() if m.group(4) else ""
        attrs = m.group(1) or ""
        
        info["types"].append((kind, name, bases))
        
        if kind == "interface":
            info["interfaces"].append(name)
    
    # Enum values
    for m in re.finditer(r'enum\s+(\w+)\s*(?::\s*\w+)?\s*\{([^}]+)\}', source):
        name = m.group(1)
        values = [v.strip().split('=')[0].strip().split(',')[-1].strip() 
                  for v in m.group(2).split(',') if v.strip() and v.strip()[0].isupper()]
        if values:
            info["enums"].append((name, values[:20]))
    
    # Fields
    for m in re.finditer(
        r'^\s+(public|internal|private|protected)\s+'
        r'(?:readonly\s+|const\s+|static\s+|volatile\s+|unsafe\s+)*'
        r'([\w<>\[\]\*?,\s]+?)\s+'
        r'(\w+)\s*(?:=|;|{)',
        source, re.MULTILINE):
        
        vis = m.group(1)
        typ = m.group(2).strip().split()[-1]  # last word is the type
        name = m.group(3)
        
        if name in ('get', 'set', 'if', 'for', 'while', 'new', 'return', 'void',
                     'class', 'struct', 'interface', 'enum', 'override', 'this',
                     'operator', 'static', 'readonly', 'abstract', 'virtual'):
            continue
        
        info["fields"].append((vis, typ[:28], name))
    
    # Methods
    for m in re.finditer(
        r'^\s+(public|internal)\s+'
        r'(?:static\s+|override\s+|virtual\s+|abstract\s+|unsafe\s+|new\s+)*'
        r'(?:readonly\s+)?'
        r'([\w<>\[\]\*?]+)\s+'
        r'(\w+)\s*'
        r'\(([^)]{0,100})\)',
        source, re.MULTILINE):
        
        vis = m.group(1)
        ret = m.group(2).strip()
        name = m.group(3).strip()
        params = m.group(4).strip()
        
        if ret in ('class', 'struct', 'interface', 'enum', 'void'):
            if ret == 'void':
                pass  # include void methods
            else:
                continue
        
        if name[0].isupper() or name.startswith("operator"):
            info["methods"].append((vis, ret[:20], name, params[:60]))
    
    return info


def build_box(title, lines, width=W):
    """Build a perfectly aligned box."""
    inner = width - 4  # "│ ... │"
    result = []
    result.append("┌" + "─" * (width - 2) + "┐")
    
    if title:
        t = f" {title} "
        pad = inner - len(t)
        if pad > 0:
            t += " " * pad
        result.append("│" + t[:inner] + "│")
        result.append("├" + "─" * (width - 2) + "┤")
    
    for line in lines:
        # Handle long lines by wrapping
        if len(line) > inner:
            wrapped = textwrap.wrap(line, inner - 2)
            for wl in wrapped:
                padded = f" {wl}"
                result.append("│" + padded.ljust(inner) + "│")
        else:
            result.append("│" + line.ljust(inner) + "│")
    
    result.append("└" + "─" * (width - 2) + "┘")
    return "\n".join(result)


def build_struct_diagram(type_name, fields, width=W):
    """Build a struct layout box."""
    inner = width - 4
    lines = []
    
    type_w = 28
    name_w = inner - type_w - 4
    
    for vis, typ, name in fields[:15]:
        marker = "+" if vis == "public" else "-" if vis == "private" else "#"
        line = f"  {marker} {typ:<{type_w-3}} {name}"
        lines.append(line[:inner])
    
    return build_box(type_name, lines, width)


def build_methods_box(methods, width=W):
    """Build methods listing box."""
    inner = width - 4
    lines = []
    
    for vis, ret, name, params in methods[:12]:
        sig = f"  {name}({params})"
        if len(sig) > inner - 2:
            sig = sig[:inner - 5] + "..."
        lines.append(sig)
        ret_line = f"    → {ret}"
        lines.append(ret_line[:inner])
    
    return build_box("Key Methods", lines, width)


def build_enum_box(name, values, width=W):
    """Build enum values box."""
    inner = width - 4
    lines = []
    
    for i in range(0, len(values), 4):
        chunk = values[i:i+4]
        line = "  " + "  |  ".join(chunk)
        lines.append(line[:inner])
    
    return build_box(f"enum {name}", lines, width)


def build_inheritance_diagram(types, width=W):
    """Build an inheritance/implementation diagram."""
    inner = width - 4
    lines = []
    
    for kind, name, bases in types[:6]:
        if bases:
            bases_short = bases[:50] + ("..." if len(bases) > 50 else "")
            lines.append(f"  {kind} {name}")
            lines.append(f"    : {bases_short}")
        else:
            lines.append(f"  {kind} {name}")
        lines.append("")
    
    return build_box("Type Hierarchy", lines, width)


def build_call_flow(source, methods, width=W):
    """Analyze source to build a call flow diagram."""
    if not source:
        return None
    
    inner = width - 4
    
    # Find method bodies and extract calls
    calls = []
    for vis, ret, name, params in methods[:6]:
        # Find this method's body
        pattern = rf'(?:public|internal)\s+[\w\s<>]+\s+{re.escape(name)}\s*\([^)]*\)\s*(?:where\s+[^{{]+)?\s*\{{'
        m = re.search(pattern, source)
        if m:
            # Get next ~500 chars of body
            start = m.end()
            body = source[start:start+500]
            
            # Extract method calls
            inner_calls = re.findall(r'(\w+)\s*\(', body)
            inner_calls = [c for c in inner_calls if c[0].islower() and c not in ('if', 'for', 'while', 'switch', 'new', 'return', 'throw', 'catch')]
            
            if inner_calls:
                calls.append((name, inner_calls[:3]))
    
    if not calls:
        return None
    
    lines = []
    for i, (method, callees) in enumerate(calls):
        lines.append(f"  {method}()")
        for call in callees:
            lines.append(f"    ├─ {call}()")
        if i < len(calls) - 1:
            lines.append(f"    │")
    
    return build_box("Call Flow", lines, width)


def generate_upgraded_readme(topic, source, rel_path, info):
    """Generate a fully upgraded README.md."""
    sections = []
    
    # Title / overview
    title = topic
    overview_lines = []
    if info["namespace"]:
        overview_lines.append(f" Namespace: {info['namespace']}")
    if info["types"]:
        kind, name, bases = info["types"][0]
        overview_lines.append(f" Type: {kind} {name}")
        if bases:
            overview_lines.append(f" Inherits: {bases[:60]}")
    if info["has_burst"]:
        overview_lines.append(f" Burst: [BurstCompile] compatible")
    if info["has_unsafe"]:
        overview_lines.append(f" Unsafe: uses unsafe code / pointers")
    overview_lines.append(f" Lines: {info['line_count']}")
    
    sections.append(build_box(title, overview_lines))
    
    # Type hierarchy
    if len(info["types"]) > 1:
        sections.append(build_inheritance_diagram(info["types"]))
    
    # Struct layout
    if info["fields"]:
        main_type = info["types"][0][1] if info["types"] else topic
        sections.append(build_struct_diagram(main_type, info["fields"]))
    
    # Enums
    for ename, values in info["enums"]:
        sections.append(build_enum_box(ename, values))
    
    # Methods
    if info["methods"]:
        sections.append(build_methods_box(info["methods"]))
    
    # Call flow
    flow = build_call_flow(source, info["methods"])
    if flow:
        sections.append(flow)
    
    # Build the README
    readme = f"# {topic}\n\n## Inner Workings Diagram\n\n"
    
    for section in sections:
        readme += "```\n" + section + "\n```\n\n"
    
    # Source links
    readme += "## Source\n\n"
    readme += f"- [{rel_path}]({GITLAB}/{rel_path})\n\n"
    
    return readme


def upgrade_branch(branch, topic_sources):
    """Upgrade a single branch's README."""
    topic = branch.replace("topic/", "").replace("_", ".")
    
    # Get source path
    rel_path = topic_sources.get(topic)
    if not rel_path:
        return "SKIP", "no source mapping"
    
    # Read source
    source = read_source(rel_path)
    if not source:
        return "SKIP", "source file not found"
    
    # Check current README quality
    current = read_branch_readme(branch)
    if not current:
        return "SKIP", "no README"
    
    line_count = current.count('\n')
    
    # Extract info
    info = extract_type_info(source)
    
    # Generate upgraded README
    upgraded = generate_upgraded_readme(topic, source, rel_path, info)
    
    if upgraded.count('\n') <= line_count:
        return "SKIP", "already detailed enough"
    
    # Checkout and write
    subprocess.run(["git", "checkout", branch], capture_output=True, cwd=REPO)
    readme_path = os.path.join(REPO, "README.md")
    with open(readme_path, "w") as f:
        f.write(upgraded)
    
    subprocess.run(["git", "add", "README.md"], capture_output=True, cwd=REPO)
    subprocess.run(
        ["git", "commit", "-m", f"Upgrade {branch} with detailed ASCII diagram"],
        capture_output=True, cwd=REPO
    )
    
    return "OK", f"upgraded ({line_count} → {upgraded.count(chr(10))} lines)"


def main():
    batch_num = int(sys.argv[1]) if len(sys.argv) > 1 else 1
    batch_size = 20
    
    with open(os.path.join(REPO, "topic_sources.json")) as f:
        topic_sources = json.load(f)
    
    r = subprocess.run(["git", "branch"], capture_output=True, text=True, cwd=REPO)
    branches = [b.strip().lstrip("* ") for b in r.stdout.strip().split("\n") if b.strip().startswith("topic/")]
    
    # Find basic diagrams
    basic = []
    for b in branches:
        readme = read_branch_readme(b)
        if readme and readme.count('\n') < 30:
            basic.append(b)
    
    start = (batch_num - 1) * batch_size
    end = start + batch_size
    batch = basic[start:end]
    
    if not batch:
        print(f"Batch {batch_num}: No basic diagrams to upgrade")
        return
    
    print(f"Batch {batch_num}: {len(batch)} upgrades ({start+1}-{start+len(batch)} of {len(basic)} basic)")
    print("─" * 60)
    
    subprocess.run(["git", "checkout", "main"], capture_output=True, cwd=REPO)
    
    ok = skip = err = 0
    for i, branch in enumerate(batch, 1):
        status, msg = upgrade_branch(branch, topic_sources)
        subprocess.run(["git", "checkout", "main"], capture_output=True, cwd=REPO)
        
        sym = {"OK": "+", "SKIP": "o", "ERROR": "X"}.get(status, "?")
        print(f"  [{i:2d}/{len(batch)}] {sym} {branch}: {msg}")
        
        if status == "OK": ok += 1
        elif status == "SKIP": skip += 1
        else: err += 1
    
    print(f"\nBatch {batch_num}: {ok} upgraded, {skip} skipped, {err} errors")


if __name__ == "__main__":
    main()
