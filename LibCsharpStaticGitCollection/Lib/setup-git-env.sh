#!/bin/sh
# Portable script to setup Git environment
# Works in sh, bash, dash, etc.

# Detect if script is being sourced
# If $0 == $BASH_SOURCE or $0 == script name -> not sourced

posix=1
if [ -n "$BASH_VERSION" ] ; then
    posix=0
fi

sourced=1
if [ "${posix}" -eq 0 ] ; then
    # Bash
    if [ "$0" = "${BASH_SOURCE[0]}" ] ; then
        sourced=0
    fi
else
    # Generic POSIX: check if $0 ends with script name
    case "$0" in
        */setup-git-env.sh|setup-git-env.sh)
            sourced=0
            ;;
    esac
fi

if [ "${sourced}" -eq 0 ]; then
    echo "This script must be sourced, not executed."
	if [ "${posix}" -eq 0 ] ; then
        echo "Run: source $0"
	else
        echo "Run: . $0"
	fi
    exit 1
fi

# Determine script directory, works when sourced in Bash and POSIX
if [ -n "$BASH_VERSION" ]; then
    # Bash: use BASH_SOURCE
    SCRIPT_PATH="${BASH_SOURCE[0]}"
else
    # POSIX: fallback, only works if script executed
    SCRIPT_PATH="$0"
fi

SCRIPT_DIR=$(cd "$(dirname "${SCRIPT_PATH}")" && pwd) || return 2

cd "${SCRIPT_DIR}" || return 3

# Required folders
REQUIRED="bin ca libexec share"

ALL_EXIST=1
for f in $REQUIRED; do
    if [ ! -d "$f" ]; then
        ALL_EXIST=0
        break
    fi
done

# Extract if needed (unless --no_extract passed)
if [ "${ALL_EXIST}" -eq 0 ] && [ "$1" != "--no_extract" ]; then
    tar xzf "GitLinux.tar.gz"
fi

# Export environment variables
PATH="${SCRIPT_DIR}/bin:$PATH"
GIT_PREFIX="${SCRIPT_DIR}"
GIT_EXEC_PATH="${SCRIPT_DIR}/libexec/git-core"
GIT_TEMPLATE_DIR="${SCRIPT_DIR}/share/git-core/templates"
GIT_SSL_CAINFO="${SCRIPT_DIR}/ca/ca.pem"

export PATH GIT_PREFIX GIT_EXEC_PATH GIT_TEMPLATE_DIR GIT_SSL_CAINFO

return 0
