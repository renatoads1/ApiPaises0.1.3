#To update submodules, we can use
if [[ $# -eq 0 ]] ; then
    echo 'Please, run # update_modules.sh <BRANCH_NAME>'
    exit 1
fi

git submodule foreach --recursive git checkout $1
git submodule foreach --recursive git pull