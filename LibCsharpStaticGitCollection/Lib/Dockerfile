# syntax=docker/dockerfile:1

#------------------------------------------------------------------------------
### Manual compiling and testing
#------------------------------------------------------------------------------

# Build (optional `--no-cache`)
# docker build --progress=plain -t tempimage .

# Debug
# docker build --progress=plain --target test_72_stage -t tempimage .
# docker run -it tempimage bash

# Export
# docker create --name exporttemp tempimage
# docker cp exporttemp:/build/GitLinux.tar.gz ./GitLinux.tar.gz
# docker rm exporttemp

# Extract
# tar xzf GitLinux.tar.gz

# Cleanup
# docker rmi tempimage
# docker system prune -f

# sudo
# sudo ln -s <yourPath>/git-static /opt/musl/git

# Normal user
# export PATH="<yourPath>/bin:$PATH"
# export GIT_PREFIX="<yourPath>"
# export GIT_EXEC_PATH="<yourPath>/libexec/git-core"
# export GIT_TEMPLATE_DIR="<yourPath>/share/git-core/templates/"
# export GIT_SSL_CAINFO=""<yourPath>/ca/ca.pem"

# Test
# git clone https://github.com/Chris82111/LibCsharpStaticGitCollection.git


#------------------------------------------------------------------------------
### (1) Sources
#------------------------------------------------------------------------------

FROM ubuntu:24.04 AS base_stage

# (3) Build zlib
# Update: https://zlib.net/
ENV zlibUrl="https://zlib.net/zlib-1.3.1.tar.gz"
ENV zlibSha256="9a93b2b7dfdac77ceba5a558a580e74667dd6fede4585b91eefb60f03b72df23"

# (4) Build OpenSSL
# Update: https://github.com/openssl/openssl/releases
ENV openSslUrl="https://github.com/openssl/openssl/releases/download/openssl-3.5.4/openssl-3.5.4.tar.gz"
ENV openSslSha256="967311f84955316969bdb1d8d4b983718ef42338639c621ec4c34fddef355e99"

# (5) Build libpsl
# Update: https://www.linuxfromscratch.org/blfs/view/svn/basicnet/libpsl.html
ENV libpslUrl="https://github.com/rockdaboot/libpsl/releases/download/0.21.5/libpsl-0.21.5.tar.gz"
ENV libpslSha256="1dcc9ceae8b128f3c0b3f654decd0e1e891afc6ff81098f227ef260449dae208"

# (6) Build libcurl (Git HTTPS support)
# Update: https://curl.se/download.html
ENV libcurlUrl="https://curl.se/download/curl-8.17.0.tar.gz"
ENV libcurlSha256="e8e74cdeefe5fb78b3ae6e90cd542babf788fa9480029cfcee6fd9ced42b7910"

# (7) Build Git
# Update: https://git-scm.com/install/source
ENV gitUrl="https://www.kernel.org/pub/software/scm/git/git-2.51.2.tar.gz"
ENV gitSha256="9b44c2b337ec838e10aad42439d390963904449710d30c9e7e4ba449f45da98f"


#------------------------------------------------------------------------------
### (2)
#------------------------------------------------------------------------------

FROM base_stage AS build_stage

RUN apt-get update -y && apt-get install -y \
  build-essential


#------------------------------------------------------------------------------
### (3) Build zlib
#------------------------------------------------------------------------------

FROM build_stage AS zlib_stage

WORKDIR /build

ADD --checksum=sha256:${zlibSha256} ${zlibUrl} packed.tar.gz

RUN \
  mkdir -p /build/zlib && \
  tar xzf "packed.tar.gz" -C "/build/zlib" --strip-components=1 && \
  rm -f "packed.tar.gz"

WORKDIR /build/zlib

# Configure
RUN ./configure --prefix=/opt/zlib --static
# Multi-threaded build, uses all CPU cores with `-j$(nproc)`
RUN make
# Install
RUN make install

# Ensure ouput is static
RUN \
  if ldd /opt/zlib/lib/libz.a >/dev/null 2>&1; then \
    echo "ERROR: libz.a is dynamically linked"; exit 1; \
  fi


#------------------------------------------------------------------------------
### (4) Build OpenSSL
#------------------------------------------------------------------------------

FROM build_stage AS openssl_stage

WORKDIR /build

ADD --checksum=sha256:${openSslSha256} ${openSslUrl} packed.tar.gz

RUN \
  mkdir -p /build/openssl && \
  tar xzf "packed.tar.gz" -C "/build/openssl" --strip-components=1 && \
  rm -f "packed.tar.gz"

WORKDIR /build/openssl

# With disable secure memory: 
# Disable AF_ALG engine during configuration
# Now configure it correctly for a static and shared build, disabling features that cause kernel header issues:
RUN ./Configure \
  shared \
  linux-x86_64 \
  no-secure-memory \
  no-afalgeng \
  --prefix=/opt/openssl \
  --openssldir=/opt/openssl

# Multi-threaded build, uses all CPU cores with `-j$(nproc)`
RUN make AR="ar" RANLIB="ranlib"

RUN make install_sw


# Ensure ouput is static
RUN \
  if ldd /opt/openssl/lib64/libcrypto.a >/dev/null 2>&1; then \
    echo "ERROR: libcrypto.a is dynamically linked"; exit 1; \
  fi

RUN \
  if ldd /opt/openssl/lib64/libssl.a >/dev/null 2>&1; then \
    echo "ERROR: libssl.a is dynamically linked"; exit 1; \
  fi

# Ensure ouput is dynamic
RUN \
  if ! readelf -d /opt/openssl/lib64/libcrypto.so >/dev/null 2>&1; then \
    echo "ERROR: libcrypto.so is NOT dynamically linked"; exit 1; \
  fi

RUN \
  if ! readelf -d /opt/openssl/lib64/libssl.so >/dev/null 2>&1; then \
    echo "ERROR: libssl.so is NOT dynamically linked"; exit 1; \
  fi

# RUN \
#   set -e; \
#   LIB=/opt/openssl/lib64/libcrypto.so; \
#   \
#   # 1. Verify shared object header
#   if ! readelf -h "$LIB" | grep -q "Type:.*DYN"; then \
#     echo "\033[0;31mERROR: Not a shared library\033[0m"; exit 1; \
#   fi; \
#   \
#   # 2. Check for interpreter
#   INTERP=$(readelf -l "$LIB" | awk '/Requesting program interpreter/ {print $NF}'); \
#   if [ -z "$INTERP" ]; then \
#     echo "\033[1;34mOK: No ELF interpreter – static or libc-independent build\033[0m"; \
#     # Accept musl libraries (NEEDED: libc.so)
#     if readelf -d "$LIB" | grep -q "Shared library: \\[libc.so\\]"; then \
#       echo "\033[1;34mOK: musl-linked '$LIB' found\033[0m"; \
#     fi; \
#     exit 0; \
#   fi; \
#   \
#   # 3. If an interpreter exists, classify it
#   if echo "$INTERP" | grep -q "ld-musl"; then \
#     echo "OK: musl-linked\033[0m"; \
#   elif echo "$INTERP" | grep -q "ld-linux"; then \
#     echo "\033[0;31mERROR: glibc-linked\033[0m"; exit 1; \
#   else \
#     echo "\033[0;31mERROR: Unknown interpreter: $INTERP\033[0m"; exit 1; \
#   fi
# 
# RUN \
#   set -e; \
#   LIB=/opt/openssl/lib64/libssl.so; \
#   \
#   # 1. Verify shared object header
#   if ! readelf -h "$LIB" | grep -q "Type:.*DYN"; then \
#     echo "\033[0;31mERROR: Not a shared library\033[0m"; exit 1; \
#   fi; \
#   \
#   # 2. Check for interpreter
#   INTERP=$(readelf -l "$LIB" | awk '/Requesting program interpreter/ {print $NF}'); \
#   if [ -z "$INTERP" ]; then \
#     echo "\033[1;34mOK: No ELF interpreter – static or libc-independent build\033[0m"; \
#     # Accept musl libraries (NEEDED: libc.so)
#     if readelf -d "$LIB" | grep -q "Shared library: \\[libc.so\\]"; then \
#       echo "\033[1;34mOK: musl-linked '$LIB' found\033[0m"; \
#     fi; \
#     exit 0; \
#   fi; \
#   \
#   # 3. If an interpreter exists, classify it
#   if echo "$INTERP" | grep -q "ld-musl"; then \
#     echo "OK: musl-linked\033[0m"; \
#   elif echo "$INTERP" | grep -q "ld-linux"; then \
#     echo "\033[0;31mERROR: glibc-linked\033[0m"; exit 1; \
#   else \
#     echo "\033[0;31mERROR: Unknown interpreter: $INTERP\033[0m"; exit 1; \
#   fi


#------------------------------------------------------------------------------
### (5) Build libpsl
#------------------------------------------------------------------------------

FROM build_stage AS libpsl_stage

RUN apt-get update -y && apt-get install -y \
  python3

WORKDIR /build

ADD --checksum=sha256:${libpslSha256} ${libpslUrl} packed.tar.gz

RUN \
  mkdir -p /build/libpsl && \
  tar xzf "packed.tar.gz" -C "/build/libpsl" --strip-components=1 && \
  rm -f "packed.tar.gz"

WORKDIR /build/libpsl

RUN ./configure \
  --prefix=/opt/libpsl \
  --disable-shared \
  --enable-static

RUN make

RUN make install

# Ensure ouput is static
RUN \
  if ldd /opt/libpsl/lib/libpsl.a >/dev/null 2>&1; then \
    echo "ERROR: libpsl.a is dynamically linked"; exit 1; \
  fi


#------------------------------------------------------------------------------
### (6) Build libcurl (Git HTTPS support)
#------------------------------------------------------------------------------

FROM build_stage AS libcurl_stage

RUN apt-get -y update && apt-get install -y \
  gettext \
  autoconf \
  automake \
  libtool \
  pkg-config

WORKDIR /build

ADD --checksum=sha256:${libcurlSha256} ${libcurlUrl} packed.tar.gz

RUN \
  mkdir -p /build/curl && \
  tar xzf "packed.tar.gz" -C "/build/curl" --strip-components=1 && \
  rm -f "packed.tar.gz"

COPY --from=zlib_stage    /opt/zlib /opt/zlib
COPY --from=openssl_stage /opt/openssl /opt/openssl
COPY --from=libpsl_stage  /opt/libpsl /opt/libpsl

WORKDIR /build/curl

ENV LD_LIBRARY_PATH="/opt/openssl/lib64:${LD_LIBRARY_PATH:+${LD_LIBRARY_PATH}}"

ENV CPPFLAGS="-I/opt/openssl/include -I/opt/zlib/include -I/opt/libpsl/include"
ENV LDFLAGS="-L/opt/openssl/lib64 -L/opt/zlib/lib -L/opt/libpsl/lib"
ENV LIBS="-lssl -lcrypto -lz -lpsl"

RUN ./configure \
  --prefix=/opt/curl \
  --enable-static \
  --enable-shared \
  --with-ssl=/opt/openssl \
  --with-zlib=/opt/zlib \
  --with-libpsl=/opt/libpsl \
  --disable-ldap

RUN make
RUN make install

# Ensure ouput is static
RUN \
  if ldd /opt/curl/lib/libcurl.a >/dev/null 2>&1; then \
    echo "ERROR: libcurl.a is dynamically linked"; exit 1; \
  fi

# RUN \
#   set -e; \
#   LIB=/opt/curl/lib/libcurl.so; \
#   \
#   # 1. Verify shared object header
#   if ! readelf -h "$LIB" | grep -q "Type:.*DYN"; then \
#     echo "\033[0;31mERROR: Not a shared library\033[0m"; exit 1; \
#   fi; \
#   \
#   # 2. Check for interpreter
#   INTERP=$(readelf -l "$LIB" | awk '/Requesting program interpreter/ {print $NF}'); \
#   if [ -z "$INTERP" ]; then \
#     echo "\033[1;34mOK: No ELF interpreter – static or libc-independent build\033[0m"; \
#     # Accept musl libraries (NEEDED: libc.so)
#     if readelf -d "$LIB" | grep -q "Shared library: \\[libc.so\\]"; then \
#       echo "\033[1;34mOK: musl-linked '$LIB' found\033[0m"; \
#     fi; \
#     exit 0; \
#   fi; \
#   \
#   # 3. If an interpreter exists, classify it
#   if echo "$INTERP" | grep -q "ld-musl"; then \
#     echo "OK: musl-linked\033[0m"; \
#   elif echo "$INTERP" | grep -q "ld-linux"; then \
#     echo "\033[0;31mERROR: glibc-linked\033[0m"; exit 1; \
#   else \
#     echo "\033[0;31mERROR: Unknown interpreter: $INTERP\033[0m"; exit 1; \
#   fi

RUN \
  /opt/curl/bin/curl https://github.com >/dev/null ; if [ $? -ne 0 ] ; then \
    echo "ERROR: curl is not functioning properly and cannot be used"; exit 1; \
  fi


#------------------------------------------------------------------------------
### (6.1) Test
#------------------------------------------------------------------------------

FROM base_stage AS test_61_stage

RUN apt-get -y update && apt-get install -y \
  gettext \
  autoconf \
  automake \
  libtool \
  pkg-config

COPY --from=zlib_stage    /opt/zlib /opt/my/test/zlib
COPY --from=libpsl_stage  /opt/libpsl /opt/my/test/libpsl
  
COPY --from=libcurl_stage /opt/curl /opt/my/test/curl
COPY --from=openssl_stage /opt/openssl /opt/my/test/openssl

ENV LD_LIBRARY_PATH="/opt/my/test/openssl/lib64:${LD_LIBRARY_PATH:+${LD_LIBRARY_PATH}}"

WORKDIR /build/curl-test

# Optional test run 
RUN echo '#include <curl/curl.h> \n int main() { CURL *c = curl_easy_init(); return 0; }' > test.c
 
RUN gcc -o test_dynamic test.c \
  -I/opt/my/test/curl/include \
  -I/opt/my/test/openssl/include \
  -L/opt/my/test/zlib/lib \
  -L/opt/my/test/libpsl/lib \
  -Wl,-Bstatic -lz -lpsl \
  -L/opt/my/test/curl/lib \
  -L/opt/my/test/openssl/lib64 \
  -Wl,-Bdynamic -lcurl -lssl -lcrypto \
  -ldl -lpthread

RUN \
  if ! readelf -d /build/curl-test/test_dynamic >/dev/null 2>&1; then \
    echo "ERROR: test_dynamic is NOT dynamically linked"; exit 1; \
  fi

RUN gcc -static -o test_static test.c \
  -I/opt/my/test/curl/include \
  -I/opt/my/test/openssl/include \
  -L/opt/my/test/curl/lib \
  -L/opt/my/test/libpsl/lib \
  -L/opt/my/test/zlib/lib \
  -L/opt/my/test/openssl/lib64 \
  -lcurl -lpsl -lssl -lcrypto -lz -ldl -lpthread

RUN \
  if ldd /build/curl-test/test_static >/dev/null 2>&1; then \
    echo "ERROR: test_static is dynamically linked"; exit 1; \
  fi


#------------------------------------------------------------------------------
### (7) Build Git
#------------------------------------------------------------------------------

FROM build_stage AS git_stage

RUN apt-get update -y && apt-get install -y \
  libcurl4-openssl-dev \
  gettext \
  autoconf automake libtool pkg-config

WORKDIR /build

ADD --checksum=sha256:${gitSha256} ${gitUrl} packed.tar.gz

RUN \
  mkdir -p /build/git && \
  tar xzf "packed.tar.gz" -C "/build/git" --strip-components=1 && \
  rm -f "packed.tar.gz"

COPY --from=zlib_stage    /opt/zlib /opt/zlib
COPY --from=openssl_stage /opt/openssl /opt/openssl
COPY --from=libpsl_stage  /opt/libpsl /opt/libpsl
COPY --from=libcurl_stage /opt/curl /opt/curl

WORKDIR /build/git

ENV LD_LIBRARY_PATH="/opt/openssl/lib64:${LD_LIBRARY_PATH:+${LD_LIBRARY_PATH}}"
ENV LD_LIBRARY_PATH="/opt/curl/lib:${LD_LIBRARY_PATH:+${LD_LIBRARY_PATH}}"

# #Check if necessary
# ENV PKG_CONFIG_PATH="/opt/zlib/lib/pkgconfig:${PKG_CONFIG_PATH}"
# ENV PKG_CONFIG_PATH="/opt/openssl/lib64/pkgconfig:${PKG_CONFIG_PATH}"
# ENV PKG_CONFIG_PATH="/opt/libpsl/lib/pkgconfig:${PKG_CONFIG_PATH}"
# ENV PKG_CONFIG_PATH="/opt/curl/lib/pkgconfig:${PKG_CONFIG_PATH}"
# 
# RUN set -e; \
#   if ! pkg-config --exists openssl; then \
#     echo "ERROR: openssl.pc missing"; \
# 	exit 1; \
#   fi; \
#   if ! pkg-config --exists zlib; then \
#     echo "ERROR: zlib.pc missing"; \
# 	exit 1; \
#   fi; \
#   if ! pkg-config --exists libpsl; then \
#     echo "ERROR: libpsl.pc missing"; \
# 	exit 1; \
#   fi; \
#   if ! pkg-config --exists libcurl; then \
#     echo "ERROR: libcurl.pc missing"; \
# 	exit 1; \
#   fi; \
#   echo "All pkg-config dependencies OK."
# 
# 
# RUN pkg-config --libs --cflags libcurl

RUN make configure

RUN ./configure \
  --prefix=/opt/git \
  --with-curl=/opt/curl \
  --with-openssl=/opt/openssl \
  --without-expat \
  NO_TCLTK=YesPlease \
  CFLAGS="-O2 -g0 -I/opt/zlib/include -I/opt/libpsl/include" \
  LDFLAGS="-L/opt/zlib/lib -L/opt/libpsl/lib -Wl,-Bstatic -lz -lpsl \
           -Wl,-Bdynamic -L/opt/curl/lib -L/opt/openssl/lib64 -lcurl -lssl -lcrypto -ldl -lpthread"

RUN make \
  NO_INSTALL_HARDLINKS=YesPlease

RUN make install \
  NO_INSTALL_HARDLINKS=YesPlease \
  prefix=/opt/git

RUN \
  find /opt/git -type f -exec sh -c \
    'file "$1" | grep -q ELF && strip "$1" || true' _ {} \; \
  && find /opt/git -name "*.a" -delete

RUN \
  mkdir -p /opt/git/ca && \
  cp /etc/ssl/certs/ca-certificates.crt /opt/git/ca/ca.pem
 
ENV PATH="/opt/git/bin:$PATH"
ENV GIT_PREFIX="/opt/git"
ENV GIT_EXEC_PATH="/opt/git/libexec/git-core"
ENV GIT_TEMPLATE_DIR="/opt/git/share/git-core/templates/"
ENV GIT_SSL_CAINFO="/opt/git/ca/ca.pem"

# allow Git to find libssl.so and libcrypto.so at runtime
ENV LD_LIBRARY_PATH="/opt/openssl/lib64:${LD_LIBRARY_PATH:+${LD_LIBRARY_PATH}}"
ENV LD_LIBRARY_PATH="/opt/curl/lib:${LD_LIBRARY_PATH:+${LD_LIBRARY_PATH}}"

WORKDIR /build/git-test

# Check that curl and OpenSSL is dynamically linked
RUN \
  if ! ldd /opt/git/libexec/git-core/git-remote-https | grep -q "libssl.so"; then \
    echo "ERROR: Git is not dynamically linked with curl"; exit 1; \
  fi

RUN \
  if ! ldd /opt/git/libexec/git-core/git-remote-https | grep -q "libcrypto.so"; then \
    echo "ERROR: Git is not dynamically linked with OpenSSL"; exit 1; \
  fi 

# Test run
RUN git --version || (echo "ERROR:"; exit 1)

RUN \
  git clone https://github.com/Chris82111/LibCsharpStaticGitCollection.git || \
  (echo "ERROR:"; exit 1)


#------------------------------------------------------------------------------
### (7.1) Test
#------------------------------------------------------------------------------

FROM base_stage AS test_71_stage

COPY --from=git_stage /opt/git /opt/my/test/git
COPY --from=libcurl_stage /opt/curl /opt/my/test/curl
COPY --from=openssl_stage /opt/openssl /opt/my/test/openssl

ENV LD_LIBRARY_PATH="/opt/my/test/openssl/lib64:${LD_LIBRARY_PATH:+${LD_LIBRARY_PATH}}"
ENV LD_LIBRARY_PATH="/opt/my/test/curl/lib:${LD_LIBRARY_PATH:+${LD_LIBRARY_PATH}}"

ENV PATH="/opt/my/test/curl/bin/:$PATH"
ENV PATH="/opt/my/test/git/bin:$PATH"
ENV GIT_PREFIX="/opt/my/test/git"
ENV GIT_EXEC_PATH="/opt/my/test/git/libexec/git-core"
ENV GIT_TEMPLATE_DIR="/opt/my/test/git/share/git-core/templates/"
ENV GIT_SSL_CAINFO="/opt/my/test/git/ca/ca.pem"

WORKDIR /build/git-test

RUN \
  git --version ; if [ $? -ne 0 ] ; then \
    echo "ERROR: git --version command failed"; exit 1; \
  fi

RUN \
  git clone https://github.com/Chris82111/LibCsharpStaticGitCollection.git ; if [ $? -ne 0 ] ; then \
    echo "ERROR: git clone ... command failed"; exit 1; \
  fi

RUN \
  if [ ! -d /build/git-test/LibCsharpStaticGitCollection ] ; then \
    echo "ERROR: git clone ... failed"; exit 1; \
  fi
  
RUN \
  if [ ! -f /build/git-test/LibCsharpStaticGitCollection/README.md ] ; then \
    echo "ERROR: git clone ... failed"; exit 1; \
  fi


#------------------------------------------------------------------------------
### (7.2) Test
#------------------------------------------------------------------------------

FROM base_stage AS test_72_stage

COPY --from=git_stage /opt/git /opt/my/test/git
COPY --from=libcurl_stage /opt/curl /opt/my/test/git/curl
COPY --from=openssl_stage /opt/openssl /opt/my/test/git/openssl
ADD setup-git-env.sh /opt/my/test/git

WORKDIR /build/git-test

RUN bash -c "\
    source /opt/my/test/git/setup-git-env.sh && \
    git --version || { \
        echo 'ERROR: git command failed'; \
        exit 1; \
    }\
"

RUN bash -c "\
    source /opt/my/test/git/setup-git-env.sh && \
    git clone https://github.com/Chris82111/LibCsharpStaticGitCollection.git || { \
        echo 'ERROR: git clone ... command failed'; \
        exit 1; \
    }\
"

RUN ls /build/git-test/

RUN \
  if [ ! -d /build/git-test/LibCsharpStaticGitCollection ] ; then \
    echo "ERROR: git clone ... failed"; exit 1; \
  fi
  
RUN \
  if [ ! -f /build/git-test/LibCsharpStaticGitCollection/README.md ] ; then \
    echo "ERROR: git clone ... failed"; exit 1; \
  fi


#------------------------------------------------------------------------------
### (8) Archive
#------------------------------------------------------------------------------

FROM base_stage

COPY --from=git_stage /opt/git /opt/git
COPY --from=libcurl_stage /opt/curl /opt/git/curl
COPY --from=openssl_stage /opt/openssl /opt/git/openssl
ADD setup-git-env.sh /opt/git

# The file `setup-git-env.sh` sets the following variables. 
# You can use the script or do this yourself:
#
# export PATH="/opt/git/bin:$PATH"
# export GIT_PREFIX="/opt/git"
# export GIT_EXEC_PATH="/opt/git/libexec/git-core"
# export GIT_TEMPLATE_DIR="/opt/git/share/git-core/templates/"
# export GIT_SSL_CAINFO="/opt/git/ca/ca.pem"
# export LD_LIBRARY_PATH="/opt/git/openssl/lib64:${LD_LIBRARY_PATH:+${LD_LIBRARY_PATH}}"
# export LD_LIBRARY_PATH="/opt/git/curl/lib:${LD_LIBRARY_PATH:+${LD_LIBRARY_PATH}}"

# WORKDIR /opt/git
# RUN find . -type f -perm -111 -exec sh -c 'file "$0" | grep -q ELF && strip "$0"' {} \;

WORKDIR /build
RUN tar czvf GitLinux.tar.gz -C /opt/git .


#------------------------------------------------------------------------------
### End
#------------------------------------------------------------------------------
