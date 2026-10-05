#!/usr/bin/env python3
import os
import re
import sys

# Regex pour identifier class / struct / enum / interface / record (compte aussi les types
# imbriqués). Tous les modificateurs sont acceptés dans n'importe quel ordre : l'ancienne
# version ne reconnaissait ni "sealed", ni "static", ni "abstract", ni "readonly" et laissait
# donc passer la plupart des violations.
DECLARATION_RE = re.compile(
    r'^\s*(?:\[[^\]]*\]\s*)*'
    r'(?:(?:public|internal|protected|private|file|sealed|static|abstract|partial|readonly|unsafe|new|ref)\s+)*'
    r'(?:class|struct|enum|interface|record)\s+\w+'
)

SKIP_DIRS = {'.git', '.github', 'bin', 'obj', '.vs', '.idea'}

def read_text_with_fallback(path: str) -> str:
    """Lit un fichier texte .cs avec détection simple d'encodage et repli."""
    with open(path, 'rb') as bf:
        raw = bf.read()

    # Fichiers probablement binaires : beaucoup de NULs et aucun \n
    if raw and raw.count(b'\x00') > max(8, len(raw) // 10) and b'\n' not in raw:
        # On considère que ce n'est pas un texte C#
        return ""

    # BOMs courants
    if raw.startswith(b'\xef\xbb\xbf'):
        return raw.decode('utf-8-sig', errors='strict')
    if raw.startswith(b'\xff\xfe'):
        return raw.decode('utf-16-le', errors='strict')
    if raw.startswith(b'\xfe\xff'):
        return raw.decode('utf-16-be', errors='strict')

    # Essais sans BOM
    for enc in ('utf-8', 'cp1252', 'latin-1'):
        try:
            return raw.decode(enc, errors='strict')
        except UnicodeDecodeError:
            pass

    # Dernier repli : on ignore les octets illisibles
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
        # filtre répertoires à ignorer
        dirnames[:] = [d for d in dirnames if d not in SKIP_DIRS]
        for fname in filenames:
            if not fname.endswith(".cs"):
                continue
            fpath = os.path.join(dirpath, fname)
            count = check_file(fpath)
            if count > 1:
                errors.append((fpath, count))

    if errors:
        print("❌ Files with more than one type (class/struct/enum/interface/record):")
        for path, count in errors:
            print(f"  {path}: {count} top-level types")
        sys.exit(1)
    else:
        print("✅ All .cs files contain at most one type (class/struct/enum/interface/record).")
        sys.exit(0)

if __name__ == "__main__":
    main(sys.argv[1] if len(sys.argv) > 1 else ".")
