FROM mono:5.8

RUN apt-get update && apt-get install -y git ssh tar gzip ca-certificates wget
RUN curl -sL https://deb.nodesource.com/setup_8.x | bash -E -
RUN apt-get install -y nodejs
RUN wget https://mediaarea.net/repo/deb/repo-mediaarea_1.0-5_all.deb && dpkg -i repo-mediaarea_1.0-5_all.deb && apt-get update
RUN apt-get install -y libmediainfo-dev libmediainfo0 mediainfo
RUN npm i -g npm
