# syntax=docker/dockerfile:1
FROM ubuntu:24.04

# Build
# docker build --progress=plain -t tempimage .

# Debug
# docker run -it tempimage bash

# Export
# docker create --name exporttemp tempimage
# docker cp exporttemp:/opt/musl/git ./git-static
# docker rm exporttemp

# Cleanup
# docker rmi tempimage
# docker system prune -f


#--no-cache

RUN apt -y update && apt install -y \
  build-essential \
  autoconf \
  curl \
  gettext \
  musl \
  musl-tools \
  zlib1g-dev \
  libssl-dev \
  libcurl4-openssl-dev \
  libexpat1-dev

#RUN which tar
RUN mkdir -p "/build"
WORKDIR /build

# (1) Build static zlib
WORKDIR /build
RUN curl -O https://zlib.net/zlib-1.3.1.tar.gz
RUN tar xzf zlib-1.3.1.tar.gz
WORKDIR /build/zlib-1.3.1
RUN CC=musl-gcc ./configure --prefix=/opt/musl/zlib --static
RUN make
RUN make install
RUN \
  if ldd /opt/musl/zlib/lib/libz.a >/dev/null 2>&1; then \
    echo "ERROR: libz.a is dynamically linked"; exit 1; \
  fi
  
#(2) Build static OpenSSL
WORKDIR /build
RUN curl -LO https://github.com/openssl/openssl/releases/download/openssl-3.5.4/openssl-3.5.4.tar.gz
RUN tar xzf openssl-3.5.4.tar.gz
WORKDIR /build/openssl-3.5.4
# with disable secure memory: 
# Disable AF_ALG engine during configuration
# Now configure it correctly for a musl static build, disabling features that cause kernel header issues:
RUN ./Configure linux-x86_64 no-secure-memory no-afalgeng no-shared no-dso --prefix=/opt/musl/openssl --openssldir=/opt/musl/openssl
RUN make CC=musl-gcc AR="ar" RANLIB="ranlib"
RUN make install_sw
RUN \
  if ldd /opt/musl/openssl/lib64/libcrypto.a >/dev/null 2>&1; then \
    echo "ERROR: libcrypto.a is dynamically linked"; exit 1; \
  fi
RUN \
  if ldd /opt/musl/openssl/lib64/libssl.a >/dev/null 2>&1; then \
    echo "ERROR: libssl.a is dynamically linked"; exit 1; \
  fi

# (4) Build static OpenSSH
WORKDIR /build
# https://www.openssh.org/portable.html ; Germany (Berlin)
RUN curl -O https://ftp.spline.de/pub/OpenBSD/OpenSSH/portable/openssh-10.0p1.tar.gz
RUN tar xzf openssh-10.0p1.tar.gz
WORKDIR /build/openssh-10.0p1

# export
ENV CC=musl-gcc
ENV CPPFLAGS="-I/build/openssl-3.5.4/include -I/opt/musl/openssl/include -I/opt/musl/zlib/include"
ENV LDFLAGS="-static -L/build/openssl-3.5.4 -L/opt/musl/openssl/lib -L/opt/musl/zlib/lib"
ENV LIBS="-lssl -lcrypto -lz -ldl -lpthread"

RUN autoreconf -i

RUN ./configure \
  --prefix=/opt/musl/ssh \
  --with-ssl-dir=/build/openssl-3.5.4 \
  --without-pam \
  --without-shadow \
  --disable-strip \
  --host=x86_64-linux-musl \
  --disable-etc-default-login

RUN make -j$(nproc)
# Create the sshd user (for full server functionality), This will satisfy the privilege separation check.
RUN useradd -r -d /var/empty -s /usr/sbin/nologin sshd
RUN make install

#/opt/musl/ssh/bin/ssh -V
RUN \
  if ldd /opt/musl/ssh/bin/ssh >/dev/null 2>&1; then \
    echo "ERROR: ssh is dynamically linked"; exit 1; \
  fi

# (5) Build libpsl
#apt update
RUN apt -y update && apt install -y \
  python3
  
WORKDIR /build
RUN curl -LO https://github.com/rockdaboot/libpsl/releases/download/0.21.5/libpsl-0.21.5.tar.gz
RUN tar xzf libpsl-0.21.5.tar.gz
WORKDIR /build/libpsl-0.21.5
RUN ./configure --prefix=/opt/musl/libpsl --disable-shared --enable-static
RUN make CC=musl-gcc
RUN make install

RUN \
  if ldd /opt/musl/libpsl/lib/libpsl.a >/dev/null 2>&1; then \
    echo "ERROR: libpsl.a is dynamically linked"; exit 1; \
  fi

# (6) libcurl (Git HTTPS support)
WORKDIR /build
RUN curl -O https://curl.se/download/curl-8.17.0.tar.gz
RUN tar xzf curl-8.17.0.tar.gz
WORKDIR /build/curl-8.17.0

ENV CC=musl-gcc
ENV CPPFLAGS="-I/build/openssl-3.5.4/include -I/opt/musl/ssl/include -I/opt/musl/zlib/include"
ENV LDFLAGS="-static -L/build/openssl-3.5.4 -L/opt/musl/ssl/lib -L/opt/musl/zlib/lib"
ENV LIBS="-lssl -lcrypto -lz -ldl -lpthread"

RUN ./configure --prefix=/opt/musl/curl \
            --disable-shared \
            --enable-static \
            --with-ssl=/build/openssl-3.5.4 \
            --with-zlib=/opt/musl/zlib \
            --with-libpsl=/opt/musl/libpsl \
            --disable-ldap \
            --disable-ldap-sasl
			
RUN make CC=musl-gcc
RUN make install

RUN \
  if ldd /opt/musl/curl/lib/libcurl.a >/dev/null 2>&1; then \
    echo "ERROR: libcurl.a is dynamically linked"; exit 1; \
  fi

#Test 
#echo '#include <curl/curl.h>
#int main() { CURL *c = curl_easy_init(); return 0; }' > test.c
#
#musl-gcc -static -o test_static test.c \
#    -I/opt/musl/curl/include \
#    -I/build/openssl-3.5.4/include \
#    -L/opt/musl/curl/lib \
#    -L/opt/musl/libpsl/lib \
#    -L/opt/musl/zlib/lib \
#    -L/build/openssl-3.5.4 \
#    -lcurl -lpsl -lssl -lcrypto -lz -ldl -lpthread

# (7) Git
WORKDIR /build
RUN curl -LO https://www.kernel.org/pub/software/scm/git/git-2.51.2.tar.gz
RUN tar xzf git-2.51.2.tar.gz
WORKDIR /build/git-2.51.2

ENV CC=musl-gcc
ENV CFLAGS="-static -I/opt/musl/zlib/include -I/opt/musl/libpsl/include -I/opt/musl/curl/include -I/build/openssl-3.5.4/include"
ENV LDFLAGS="-static -L/opt/musl/zlib/lib -L/opt/musl/libpsl/lib -L/opt/musl/curl/lib -L/build/openssl-3.5.4"
ENV LIBS="-lz -lpsl -lcurl -lssl -lcrypto -lpthread -ldl"

RUN ./configure \
  --prefix=/opt/git-static \
  --with-curl=/opt/musl/curl \
  --with-openssl=/build/openssl-3.5.4 \
  --without-expat \
  NO_TCLTK=YesPlease

RUN make \
    CC=musl-gcc \
    CFLAGS="-static" \
    LDFLAGS="-static -L/opt/musl/curl/lib -L/opt/musl/libpsl/lib -L/opt/musl/zlib/lib -L/build/openssl-3.5.4" \
    EXTLIBS="-lpsl -lcurl -lssl -lcrypto -lz -lpthread -ldl"

RUN make install \
    prefix=/opt/musl/git \
    CC=musl-gcc \
    CFLAGS="-static" \
    LDFLAGS="-static -L/opt/musl/curl/lib -L/opt/musl/libpsl/lib -L/opt/musl/zlib/lib -L/build/openssl-3.5.4" \
    EXTLIBS="-lpsl -lcurl -lssl -lcrypto -lz -lpthread -ldl"

# /opt/musl/git/bin/git --version
RUN \
  if ldd /opt/musl/git/bin/git >/dev/null 2>&1; then \
    echo "ERROR: git is dynamically linked"; exit 1; \
  fi

CMD ["echo", "Done"]