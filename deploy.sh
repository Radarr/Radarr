if [ -z "$CIRCLE_PULL_REQUEST" ]; then
    echo "We are building a normal branch, deploying as such..."
    curl "http://pr.radarr.video:4466/deploy?url=https%3A%2F%2F${CIRCLE_BUILD_NUM}-77323220-gh.circle-artifacts.com%2F0%2Fartifacts%2FRadarr.${CIRCLE_BRANCH//\//-}.$BUILD_VERSION.$CIRCLE_BUILD_NUM.linux.tar.gz&b=branch&name=${CIRCLE_BRANCH}"
else
    echo "We are building a pr, deploying as such..."
    curl "http://pr.radarr.video:4466/deploy?url=https%3A%2F%2F${CIRCLE_BUILD_NUM}-77323220-gh.circle-artifacts.com%2F0%2Fartifacts%2FRadarr.${CIRCLE_BRANCH//\//-}.$BUILD_VERSION.$CIRCLE_BUILD_NUM.linux.tar.gz&b=pr&name=${CIRCLE_PR_NUMBER}"
fi