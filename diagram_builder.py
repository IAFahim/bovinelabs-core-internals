"""
ASCII Architecture Diagram Builder for BovineLabs Core Internals.
Uses Rich library for perfectly aligned box-drawing characters.
Outputs plain text (no ANSI) suitable for README.md code blocks.
"""

import io
from rich.console import Console
from rich.panel import Panel
from rich.table import Table
from rich.tree import Tree
from rich.text import Text
from rich.columns import Columns
from rich import box
from rich.rule import Rule


def make_console(width=72):
    """Create a plain-text console (no ANSI codes)."""
    buf = io.StringIO()
    c = Console(file=buf, width=width, legacy_windows=False, force_terminal=False, no_color=True)
    return c, buf


def render_panels(panels, width=72, spacing=1):
    """Render multiple panels side-by-side or stacked."""
    c, buf = make_console(width)
    for i, (title, content) in enumerate(panels):
        c.print(Panel(content, title=title, box=box.SQUARE, padding=(0, 1)))
        if i < len(panels) - 1 and spacing:
            c.print()
    return buf.getvalue()


def render_comparison_table(title, headers, rows, width=72):
    """Render a comparison table (e.g., standard vs BovineLabs approach)."""
    c, buf = make_console(width)
    t = Table(box=box.SQUARE, show_header=True, title=title, title_justify="left")
    for h in headers:
        t.add_column(h)
    for row in rows:
        t.add_row(*row)
    c.print(t)
    return buf.getvalue()


def render_tree(root_label, children, width=72):
    """Render a tree diagram (e.g., memory layout, inheritance)."""
    c, buf = make_console(width)
    tree = Tree(root_label)
    nodes = {"": tree}

    def add(level_items, parent_key=""):
        for key, label, sub in level_items:
            parent = nodes[parent_key]
            node = parent.add(label)
            nodes[key] = node
            if sub:
                add(sub, key)

    add(children)
    c.print(tree)
    return buf.getvalue()


def render_flow(steps, width=72):
    """Render a vertical flowchart with arrows between steps."""
    c, buf = make_console(width)
    for i, (title, content) in enumerate(steps):
        c.print(Panel(content, title=f"Step {i+1}: {title}", box=box.SQUARE, padding=(0, 1)))
        if i < len(steps) - 1:
            # Arrow between steps - use padding for centering
            c.print()
            c.print("".ljust(width // 2 - 1) + "│")
            c.print("".ljust(width // 2 - 1) + "▼")
            c.print()
    return buf.getvalue()


def render_memory_layout(title, regions, width=72):
    """Render a memory layout diagram with byte offsets.
    
    regions: list of (name, size_bytes, content_description)
    """
    lines = []
    top = "┌" + "─" * (width - 4) + "┐"
    bot = "└" + "─" * (width - 4) + "┘"
    
    lines.append(f"  {title}")
    lines.append(f"  {top}")
    
    offset = 0
    for name, size, desc in regions:
        line = f"  │ [{offset:#06x}] {name} ({size}B): {desc}"
        # Pad to width
        pad = width - 6 - len(line) + 4  # account for "  │ " prefix
        if pad > 0:
            line += " " * pad
        line += "│"
        lines.append(line[:width])
        offset += size
        
        # Add separator between regions
        if name != regions[-1][0]:
            sep = "  ├" + "─" * (width - 4) + "┤"
            lines.append(sep[:width])
    
    lines.append(f"  {bot}")
    return "\n".join(lines)


def render_data_struct(name, fields, width=72):
    """Render a struct/class layout diagram.
    
    fields: list of (type, name, notes)
    """
    import textwrap
    c, buf = make_console(width)
    
    inner_w = width - 6  # space inside "│ ... │"
    
    # Calculate column widths
    type_w = max(len(f[0]) for f in fields) + 1
    name_w = max(len(f[1]) for f in fields) + 1
    notes_col = inner_w - type_w - name_w - 6  # "    type name // note"
    
    lines = []
    for typ, field_name, notes in fields:
        line = f"    {typ:<{type_w}} {field_name}"
        if notes:
            prefix = f"    {typ:<{type_w}} {field_name:<{name_w}} // "
            # Truncate notes to fit, or wrap
            avail = inner_w - len(prefix)
            if len(notes) <= avail:
                lines.append(prefix + notes)
            else:
                # Truncate with ellipsis
                lines.append(prefix + notes[:avail-3] + "...")
        else:
            lines.append(line)
    
    content = "\n".join(lines)
    c.print(Panel(f"struct {name} {{\n{content}\n}}", title=name, box=box.SQUARE, padding=(0, 1)))
    return buf.getvalue()


def build_readme(title, purpose, sections, width=72):
    """Build a complete README.md with code-block wrapping.
    
    sections: list of (section_title, ascii_content)
    """
    c, buf = make_console(width)
    
    c.print(f"# {title}")
    c.print()
    c.print("## Inner Workings Diagram")
    c.print()
    
    for section_title, content in sections:
        if section_title:
            c.print(f"### {section_title}")
            c.print()
        c.print("```")
        c.print(content.rstrip())
        c.print("```")
        c.print()
    
    return buf.getvalue()


# ── Demo / Test ──────────────────────────────────────────────

if __name__ == "__main__":
    print("=" * 72)
    print("DEMO: Architecture Diagram Builder")
    print("=" * 72)
    
    # 1. Comparison Table
    print("\n--- COMPARISON TABLE ---\n")
    print(render_comparison_table(
        "Standard vs BovineLabs",
        ["Standard Path", "BovineLabs Path"],
        [
            ("GetNativeArray<T>(isReadOnly)", "GetNativeArrayReadOnly<T>()"),
            ("Marks component as read-accessed", "Bypasses change filter tracking"),
            ("Safety handle per access", "Zero overhead in release builds"),
            ("Full dependency sync", "Skips unnecessary syncs"),
        ]
    ))
    
    # 2. Flow Diagram
    print("\n--- FLOW DIAGRAM ---\n")
    print(render_flow([
        ("Get chunk pointer", "ArchetypeChunk._GetChunkPtr()\nReturns void* to raw chunk data"),
        ("Read component versions", "chunk->GetChangeVersion(typeIndex)\nReturns uint version counter"),
        ("Compare with cached", "version != cachedVersion\nFirst call always returns true"),
        ("Update cache", "Store new version for next frame\nReturn true/false"),
    ]))
    
    # 3. Memory Layout
    print("\n--- MEMORY LAYOUT ---\n")
    print(render_memory_layout("ArchetypeChunk (16KB)", [
        ("Header", 64, "Archetype*, EntityCount, Padding"),
        ("Position[]", 4800, "400 entities x float3 (12B)"),
        ("Velocity[]", 4800, "400 entities x float3 (12B)"),
        ("Health[]", 1600, "400 entities x int (4B)"),
        ("Change Versions", 48, "uint per component type in archetype"),
    ]))
    
    # 4. Tree Diagram
    print("\n--- TREE DIAGRAM ---\n")
    print(render_tree("ECS World", [
        ("systems", "Systems", [
            ("init", "InitSystemBase (runs once)", []),
            ("update", "Update Systems", [
                ("phys", "PhysicsUpdate", []),
                ("move", "MovementSystem", []),
            ]),
        ]),
        ("entities", "Entities", [
            ("arch", "Archetypes (grouped by component layout)", [
                ("chunks", "Chunks (16KB blocks of entities)", []),
            ]),
        ]),
    ]))
    
    # 5. Data Struct
    print("\n--- DATA STRUCT ---\n")
    print(render_data_struct("EntityInChunk", [
        ("Chunk*", "Chunk", "pointer to archetype chunk"),
        ("int", "IndexInChunk", "entity index within chunk"),
    ]))
    
    # 6. Full README demo
    print("\n--- FULL README (first 50 lines) ---\n")
    readme = build_readme(
        "ArchetypeChunk.DidChange",
        "Safely checks component versions without triggering false dependencies",
        [
            ("Flow", render_flow([
                ("Get version from chunk", "Read the component's change version\nfrom the chunk's version array"),
                ("Compare with cached", "Match against previously stored version\nfrom last time this system ran"),
                ("Return result", "true = changed since last check\nfalse = unchanged (skip processing)"),
            ])),
            ("Memory", render_memory_layout("Chunk Version Tracking", [
                ("GlobalVersion", 4, "uint, incremented every write anywhere"),
                ("ComponentVersions", 48, "uint per type index in archetype"),
            ])),
            ("Comparison", render_comparison_table(
                "DidChange vs Manual Check",
                ["Manual Version Check", "DidChange Extension"],
                [
                    ("Read chunk->Versions[i]", "chunk.DidChange<T>(reqVersion)"),
                    ("Compare manually", "Handles version caching internally"),
                    ("Must know type index", "Resolves type index from generic T"),
                ]
            )),
        ]
    )
    for line in readme.split('\n')[:50]:
        print(line)
