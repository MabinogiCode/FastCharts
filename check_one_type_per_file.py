#!/usr/bin/env python3
import os
import re
import sys

# Regex to identify class / struct / enum / interface / record / delegate
# (also counts nested types)
DECLARATION_RE = re.compile(
    r'^\s*((?:public|internal|protected|private|sealed|abstract|static|partial|readonly|ref)\s+)*'
    r'(class|struct|enum|interface|record|delegate)\s+\w+'
)

SKIP_DIRS = {'.git', '.github', 'bin', 'obj', '.vs', '.idea'}

def read_text_with_fallback(path: str) -> str:
    """Reads a .cs text file with simple encoding detection and fallback."""
    with open(path, 'rb') as bf:
        raw = bf.read()

    # Probably binary files: many NULs and no \n
    if raw and raw.count(b'\x00') > max(8, len(raw) // 10) and b'\n' not in raw:
        # Consider this not to be C# text
        return ""

    # Common BOMs
    if raw.startswith(b'\xef\xbb\xbf'):
        return raw.decode('utf-8-sig', errors='strict')
    if raw.startswith(b'\xff\xfe'):
        return raw.decode('utf-16-le', errors='strict')
    if raw.startswith(b'\xfe\xff'):
        return raw.decode('utf-16-be', errors='strict')

    # Attempts without BOM
    for enc in ('utf-8', 'cp1252', 'latin-1'):
        try:
            return raw.decode(enc, errors='strict')
        except UnicodeDecodeError:
            pass

    # Last fallback: ignore unreadable bytes
    return raw.decode('utf-8', errors='ignore')

def check_file(path: str) -> int:
    text = read_text_with_fallback(path)
    if not text:
        return 0
    count = 0
    for line in text.splitlines():
        if DECLARATION_RE.match(line):
            count += 1
    return count

def main(root: str = "."):
    errors = []
    for dirpath, dirnames, filenames in os.walk(root):
        # filter out directories to ignore
        dirnames[:] = [d for d in dirnames if d not in SKIP_DIRS]
        for fname in filenames:
            if not fname.endswith(".cs"):
                continue
            fpath = os.path.join(dirpath, fname)
            count = check_file(fpath)
            if count > 1:
                errors.append((fpath, count))

    if errors:
        print("❌ Files with more than one type:")
        for path, count in errors:
            print(f"  {path}: {count} top-level types")
        sys.exit(1)
    else:
        print("✅ All .cs files contain at most one type.")
        sys.exit(0)

if __name__ == "__main__":
    main(sys.argv[1] if len(sys.argv) > 1 else ".")
