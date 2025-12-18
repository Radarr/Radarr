#!/bin/bash
# Install git hooks for Aletheia development

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
HOOKS_DIR="$SCRIPT_DIR/../.git/hooks"

echo "Installing git hooks..."

if [ -f "$HOOKS_DIR/pre-commit" ]; then
    echo "Backing up existing pre-commit hook..."
    mv "$HOOKS_DIR/pre-commit" "$HOOKS_DIR/pre-commit.backup"
fi

cp "$SCRIPT_DIR/pre-commit" "$HOOKS_DIR/pre-commit"
chmod +x "$HOOKS_DIR/pre-commit"

echo "Git hooks installed successfully."
echo "Run 'yarn install' to ensure lint dependencies are available."
