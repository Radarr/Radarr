#!/bin/bash
# Generate a Markdown change log of pull requests from commits between two tags
scriptDir=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" &>/dev/null && pwd)
ghRepo="Radarr"
branch="develop"
#read -r -p "What Repo?: " ghRepo
#read -r -p "What Org?: [Default:$ghRepo]" ghOrg
#read -r -p "What Branch?:" branch
ghOrg=${ghOrg:-$ghRepo}
ghRepoUrl=https://github.com/$ghOrg/$ghRepo

case "${branch}" in
master)
    hotioBranch='release'
    lsioBranch='latest'
    branchType='Stable'
    ;;
develop)
    hotioBranch='testing'
    lsioBranch='develop'
    branchType='Beta'
    ;;
nightly)
    hotioBranch='nightly'
    lsioBranch='nightly'
    branchType='Alpha'
    ;;
esac
baseDir=$(dirname "$scriptDir")
changelogDir="$baseDir/changelogs/"
templateDir="$changelogDir/templates/"
# Get a list of all tags in reverse order
# Assumes the tags are in version format like v1.2.3
gitTags=$(git ls-remote -t --exit-code --refs --sort='-v:refname' "$ghRepoUrl" | sed -E 's/^[[:xdigit:]]+[[:space:]]+refs\/tags\/(.+)/\1/g')

# Make the tags an array

# shellcheck disable=SC2206
tags=($gitTags)

latestTag=${tags[0]}
previousTag=${tags[1]}

# Get a log of commits that occurred between two tags
# See Pretty format placeholders at https://git-scm.com/docs/pretty-formats
# -i -E --grep="(Fixed:|New:)"'
commits=$(git log --pretty=format:' - %s%n' "$previousTag".."$latestTag")
# Store our changelog in a variable to be saved to a file at the end
markdown="# New ${branchType^} Release"
markdown+='\n\n'
markdown+="$ghRepo $latestTag has been released on \`$branch\`"
markdown+='\n\n'
branchmsg=$(cat "$templateDir"/branch-$branch.md)
if [ -n "$branchmsg" ]; then
    {
        markdown+=$branchmsg
        markdown+='\n\n'
    }
fi
markdown+="# Announcements"
markdown+='\n\n'
markdown+=$(cat "$templateDir"/announcements.md)
markdown+='\n\n'
markdown+="# Additional Commentary"
markdown+='\n\n'
markdown+=$(cat "$templateDir"/commentary.md)
markdown+='\n\n'
markdown+="# Releases"
markdown+='\n\n'
markdown+="## Native"
markdown+="\n\n"
markdown+="- [GitHub Releases]($ghRepoUrl/releases)"
markdown+="\n\n"
markdown+="- [Wiki Installation Instructions](https://wiki.servarr.com/${ghRepo,,}/installation)"
markdown+="\n\n"
markdown+="## Docker"
markdown+="\n\n"
markdown+="- [hotio/$ghRepo:$hotioBranch](https://hotio.dev/containers/${ghRepo,,})"
markdown+="\n\n"
markdown+="- [lscr.io/linuxserver/$ghRepo:$lsioBranch](https://docs.linuxserver.io/images/docker-${ghRepo,,})"
markdown+="\n\n"
markdown+="## NAS Packages"
markdown+="\n\n"
markdown+="- Synology - Please ask the SynoCommunity to update the base package; however, you can update in-app normally"
markdown+="\n\n"
markdown+="- QNAP - Please ask the SynoCommunity to update the base package; however, you should be able to update in-app normally"
markdown+="\n\n"
markdown+="------------"
markdown+="\n\n"
markdown+="# Release Notes"
markdown+="\n\n"
markdown+="## $latestTag (changes since $previousTag)"
markdown+="\n\n"
markdown+="$commits"
markdown+="\n\n"
markdown+="- Other bug fixes and improvements, see GitHub history"
# Loop over each commit and look for merged pull requests
#for COMMIT in $COMMITS; do

#done

# Save our markdown to a file
mkdir -p "$changelogDir"
echo -e "$markdown" >"$changelogDir/CHANGELOG-$latestTag.md"
exit 0
