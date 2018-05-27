FROM mono:5.8

RUN apt-get update && apt-get install -y git ssh tar gzip ca-certificates wget zip
RUN curl -sL https://deb.nodesource.com/setup_8.x | bash -E -
RUN apt-get install -y nodejs
RUN wget https://mediaarea.net/repo/deb/repo-mediaarea_1.0-5_all.deb && dpkg -i repo-mediaarea_1.0-5_all.deb && apt-get update
RUN apt-get install -y libmediainfo-dev libmediainfo0 mediainfo
RUN npm i -g npm
RUN apt-get install -y python3-pip && pip3 install gitchangelog pystache
RUN curl -O https://dl.google.com/go/go1.10.2.linux-amd64.tar.gz && tar xvf go*.tar.gz && chown -R root:root ./go && mv go /usr/local
ENV GOPATH=$HOME/work
ENV PATH="${PATH}:/usr/local/go/bin:$GOPATH/bin"
RUN go get github.com/aktau/github-release
