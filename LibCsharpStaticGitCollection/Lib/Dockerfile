# syntax=docker/dockerfile:1

# Build (optional `--no-cache`)
# docker build --progress=plain -t tempimage .

# Debug
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
### (0)
#------------------------------------------------------------------------------

FROM ubuntu:24.04 AS base_stage

# Empty

FROM base_stage AS build_stage

RUN apt update -y && apt install -y \
  build-essential \
  musl \
  musl-tools


#------------------------------------------------------------------------------
### (1) Build static zlib
#------------------------------------------------------------------------------

FROM build_stage AS zlib_stage

# Update: https://zlib.net/
ENV zlibUrl="https://zlib.net/zlib-1.3.1.tar.gz"
ENV zlibSha256="9a93b2b7dfdac77ceba5a558a580e74667dd6fede4585b91eefb60f03b72df23"

WORKDIR /build

ADD --checksum=sha256:${zlibSha256} ${zlibUrl} packed.tar.gz

RUN \
  mkdir -p /build/zlib && \
  tar xzf "packed.tar.gz" -C "/build/zlib" --strip-components=1 && \
  rm -f "packed.tar.gz"

WORKDIR /build/zlib

# Configure
RUN CC=musl-gcc ./configure --prefix=/opt/zlib --static
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
### (2) Build static OpenSSL
#------------------------------------------------------------------------------

FROM build_stage AS openssl_stage

# Update: https://github.com/openssl/openssl/releases
ENV openSslUrl="https://github.com/openssl/openssl/releases/download/openssl-3.5.4/openssl-3.5.4.tar.gz"
ENV openSslSha256="967311f84955316969bdb1d8d4b983718ef42338639c621ec4c34fddef355e99"

WORKDIR /build

ADD --checksum=sha256:${openSslSha256} ${openSslUrl} packed.tar.gz

RUN \
  mkdir -p /build/openssl && \
  tar xzf "packed.tar.gz" -C "/build/openssl" --strip-components=1 && \
  rm -f "packed.tar.gz"

WORKDIR /build/openssl

# With disable secure memory: 
# Disable AF_ALG engine during configuration
# Now configure it correctly for a musl static build, disabling features that cause kernel header issues:
RUN ./Configure linux-x86_64 no-secure-memory no-afalgeng no-shared no-dso --prefix=/opt/openssl --openssldir=/opt/openssl
# Multi-threaded build, uses all CPU cores with `-j$(nproc)`
RUN make CC=musl-gcc AR="ar" RANLIB="ranlib"
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


#------------------------------------------------------------------------------
### (3) Build static OpenSSH
#------------------------------------------------------------------------------

FROM build_stage AS ssh_stage

# Update: https://www.openssh.org/portable.html ; Germany (Berlin)
ENV openSshUrl="https://ftp.spline.de/pub/OpenBSD/OpenSSH/portable/openssh-10.0p1.tar.gz"
ENV openSshSha256="021a2e709a0edf4250b1256bd5a9e500411a90dddabea830ed59cef90eb9d85c"

RUN apt update -y && apt install -y \
  autoconf

WORKDIR /build

ADD --checksum=sha256:${openSshSha256} ${openSshUrl} packed.tar.gz

RUN \
  mkdir -p /build/ssh && \
  tar xzf "packed.tar.gz" -C "/build/ssh" --strip-components=1 && \
  rm -f "packed.tar.gz"

COPY --from=zlib_stage    /build/zlib /build/zlib
COPY --from=zlib_stage    /opt/zlib /opt/zlib
COPY --from=openssl_stage /build/openssl /build/openssl
COPY --from=openssl_stage /opt/openssl /opt/openssl

WORKDIR /build/ssh

ENV CC=musl-gcc
ENV CPPFLAGS="-I/build/openssl/include -I/opt/openssl/include -I/opt/zlib/include"
ENV LDFLAGS="-static -L/build/openssl -L/opt/openssl/lib -L/opt/zlib/lib"
ENV LIBS="-lssl -lcrypto -lz -ldl -lpthread"

RUN autoreconf -i

RUN ./configure \
  --prefix=/opt/ssh \
  --with-ssl-dir=/build/openssl \
  --without-pam \
  --without-shadow \
  --disable-strip \
  --host=x86_64-linux-musl \
  --disable-etc-default-login
  
# Multi-threaded build, uses all CPU cores with `-j$(nproc)`
RUN make
# Create the sshd user (for full server functionality), This will satisfy the privilege separation check.
RUN useradd -r -d /var/empty -s /usr/sbin/nologin sshd
RUN make install

# Optional test run
#/opt/ssh/bin/ssh -V

# Ensure ouput is static
RUN \
  if ldd /opt/ssh/bin/ssh >/dev/null 2>&1; then \
    echo "ERROR: ssh is dynamically linked"; exit 1; \
  fi


#------------------------------------------------------------------------------
### (4) Build libpsl
#------------------------------------------------------------------------------

FROM build_stage AS libpsl_stage

# Update: https://www.linuxfromscratch.org/blfs/view/svn/basicnet/libpsl.html
ENV libpslUrl="https://github.com/rockdaboot/libpsl/releases/download/0.21.5/libpsl-0.21.5.tar.gz"
ENV libpslSha256="1dcc9ceae8b128f3c0b3f654decd0e1e891afc6ff81098f227ef260449dae208"

RUN apt update -y && apt install -y \
  python3

WORKDIR /build

ADD --checksum=sha256:${libpslSha256} ${libpslUrl} packed.tar.gz

RUN \
  mkdir -p /build/libpsl && \
  tar xzf "packed.tar.gz" -C "/build/libpsl" --strip-components=1 && \
  rm -f "packed.tar.gz"

WORKDIR /build/libpsl

RUN ./configure --prefix=/opt/libpsl --disable-shared --enable-static
RUN make CC=musl-gcc
RUN make install

# Ensure ouput is static
RUN \
  if ldd /opt/libpsl/lib/libpsl.a >/dev/null 2>&1; then \
    echo "ERROR: libpsl.a is dynamically linked"; exit 1; \
  fi


#------------------------------------------------------------------------------
### (5) libcurl (Git HTTPS support)
#------------------------------------------------------------------------------

FROM build_stage AS libcurl_stage

# Update: https://curl.se/download.html
ENV libcurlUrl="https://curl.se/download/curl-8.17.0.tar.gz"
ENV libcurlSha256="e8e74cdeefe5fb78b3ae6e90cd542babf788fa9480029cfcee6fd9ced42b7910"

RUN apt -y update && apt install -y \
  gettext

WORKDIR /build

ADD --checksum=sha256:${libcurlSha256} ${libcurlUrl} packed.tar.gz

RUN \
  mkdir -p /build/curl && \
  tar xzf "packed.tar.gz" -C "/build/curl" --strip-components=1 && \
  rm -f "packed.tar.gz"

COPY --from=zlib_stage    /opt/zlib /opt/zlib
COPY --from=openssl_stage /build/openssl /build/openssl
COPY --from=openssl_stage /opt/openssl /opt/openssl
COPY --from=libpsl_stage  /opt/libpsl /opt/libpsl

WORKDIR /build/curl

ENV CC=musl-gcc
ENV CPPFLAGS="-I/build/openssl/include -I/opt/openssl/include -I/opt/zlib/include"
ENV LDFLAGS="-static -L/build/openssl -L/opt/openssl/lib -L/opt/zlib/lib"
ENV LIBS="-lssl -lcrypto -lz -ldl -lpthread"

RUN ./configure \
  --prefix=/opt/curl \
  --disable-shared \
  --enable-static \
  --with-ssl=/build/openssl \
  --with-zlib=/opt/zlib \
  --with-libpsl=/opt/libpsl \
  --disable-ldap \
  --disable-ldap-sasl

RUN make CC=musl-gcc
RUN make install

# Optional test run 
#echo '#include <curl/curl.h>
#int main() { CURL *c = curl_easy_init(); return 0; }' > test.c
#
#musl-gcc -static -o test_static test.c \
#  -I/opt/curl/include \
#  -I/build/openssl-3.5.4/include \
#  -L/opt/curl/lib \
#  -L/opt/libpsl/lib \
#  -L/opt/zlib/lib \
#  -L/build/openssl-3.5.4 \
#  -lcurl -lpsl -lssl -lcrypto -lz -ldl -lpthread

# Ensure ouput is static
RUN \
  if ldd /opt/curl/lib/libcurl.a >/dev/null 2>&1; then \
    echo "ERROR: libcurl.a is dynamically linked"; exit 1; \
  fi

RUN \
  /opt/curl/bin/curl https://github.com >/dev/null ; if [ $? -ne 0 ] ; then \
    echo "ERROR: curl is not functioning properly and cannot be used"; exit 1; \
  fi


#------------------------------------------------------------------------------
### (7) Git
#------------------------------------------------------------------------------

FROM build_stage AS git_stage

# Update: https://git-scm.com/install/source
ENV gitUrl="https://www.kernel.org/pub/software/scm/git/git-2.51.2.tar.gz"
ENV gitSha256="9b44c2b337ec838e10aad42439d390963904449710d30c9e7e4ba449f45da98f"

RUN apt update -y && apt install -y \
  libcurl4-openssl-dev \
  gettext

WORKDIR /build

ADD --checksum=sha256:${gitSha256} ${gitUrl} packed.tar.gz

RUN \
  mkdir -p /build/git && \
  tar xzf "packed.tar.gz" -C "/build/git" --strip-components=1 && \
  rm -f "packed.tar.gz"

COPY --from=zlib_stage    /build/zlib /build/zlib
COPY --from=zlib_stage    /opt/zlib /opt/zlib
COPY --from=openssl_stage /build/openssl /build/openssl
COPY --from=openssl_stage /opt/openssl /opt/openssl
COPY --from=libpsl_stage  /opt/libpsl /opt/libpsl
COPY --from=libcurl_stage /opt/curl /opt/curl

WORKDIR /build/git

# For the `./configure`, `make`, and `make install` command
ENV CC=musl-gcc
ENV CFLAGS="-static -I/opt/zlib/include -I/opt/libpsl/include -I/opt/curl/include -I/build/openssl/include"
ENV LDFLAGS="-static -L/opt/zlib/lib -L/opt/libpsl/lib -L/opt/curl/lib -L/opt/openssl/lib64"

RUN ./configure \
  --prefix=/opt/git \
  --with-curl=/opt/curl \
  --with-openssl=/opt/openssl \
  --without-expat \
  NO_TCLTK=YesPlease \
  LIBS="-lz -lpsl -lcurl -lssl -lcrypto -lpthread -ldl"

RUN make \
  NO_INSTALL_HARDLINKS=YesPlease \
  EXTLIBS="-lz -lpsl -lcurl -lssl -lcrypto -lpthread -ldl"

RUN make install \
  NO_INSTALL_HARDLINKS=YesPlease \
  prefix=/opt/git \
  EXTLIBS="-lz -lpsl -lcurl -lssl -lcrypto -lpthread -ldl"

# If the later CA of the system is used, the use and copying of a local
# CA can be omitted, as well as the use of an environment variable.
# But `libcurl4-openssl-dev` or `ca-certificates` must be installed.
RUN \
  mkdir -p /opt/git/ca && \
  cp /etc/ssl/certs/ca-certificates.crt /opt/git/ca/ca.pem

ENV PATH="/opt/git/bin:$PATH"
ENV GIT_PREFIX="/opt/git"
ENV GIT_EXEC_PATH="/opt/git/libexec/git-core"
ENV GIT_TEMPLATE_DIR="/opt/git/share/git-core/templates/"
# Tell Git to use local CA bundle
ENV GIT_SSL_CAINFO="/opt/git/ca/ca.pem"

WORKDIR /build/git-test

# Ensure ouput is static
RUN \
  if ldd /opt/git/bin/git >/dev/null 2>&1; then \
    echo "ERROR: git is dynamically linked"; exit 1; \
  fi

# Test run
RUN \
  git --version ; if [ $? -ne 0 ] ; then \
    echo "ERROR:"; exit 1; \
  fi

RUN \
  git clone https://github.com/Chris82111/LibCsharpStaticGitCollection.git ; if [ $? -ne 0 ] ; then \
    echo "ERROR:"; exit 1; \
  fi


#------------------------------------------------------------------------------
### (8) Test
#------------------------------------------------------------------------------

FROM base_stage

COPY --from=git_stage /opt/git /opt/my/test/git

ENV PATH="/opt/my/test/git/bin:$PATH"
ENV GIT_PREFIX="/opt/my/test/git"
ENV GIT_EXEC_PATH="/opt/my/test/git/libexec/git-core"
ENV GIT_TEMPLATE_DIR="/opt/my/test/git/share/git-core/templates/"
ENV GIT_SSL_CAINFO="/opt/my/test/git/ca/ca.pem"

WORKDIR /build/git-test

# Test run
RUN \
  git --version ; if [ $? -ne 0 ] ; then \
    echo "ERROR:"; exit 1; \
  fi

RUN \
  git clone https://github.com/Chris82111/LibCsharpStaticGitCollection.git ; if [ $? -ne 0 ] ; then \
    echo "ERROR:"; exit 1; \
  fi


#------------------------------------------------------------------------------
### (9) Archive
#------------------------------------------------------------------------------

FROM base_stage

COPY --from=git_stage /opt/git /opt/git

# ENV PATH="/opt/my/test/git/bin:$PATH"
# ENV GIT_PREFIX="/opt/my/test/git"
# ENV GIT_EXEC_PATH="/opt/my/test/git/libexec/git-core"
# ENV GIT_TEMPLATE_DIR="/opt/my/test/git/share/git-core/templates/"
# ENV GIT_SSL_CAINFO="/opt/my/test/git/ca/ca.pem"

# WORKDIR /opt/git
# RUN find . -type f -perm -111 -exec sh -c 'file "$0" | grep -q ELF && strip "$0"' {} \;

WORKDIR /build
RUN tar czvf GitLinux.tar.gz -C /opt/git .


#------------------------------------------------------------------------------
### End
#------------------------------------------------------------------------------
