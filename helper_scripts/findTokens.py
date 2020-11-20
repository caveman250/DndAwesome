from os import listdir
from os.path import isfile, join
import re
import difflib

def main():
    all_token_files = listdir("../images/dnd_tokens")
    token_paths = []
    for token in all_token_files:
        #token = join("images/dnd_tokens/", token)
        token = token[:-4]
        token_paths.append(token)
    print(token_paths)

    bestDiffScore = -1
    chosenTokenFile = ""
    for token in token_paths:
        split = re.findall('[A-Z][^A-Z]*', token)
        splitSearchTerm = "Mind Flayer".split(' ')
        
        #remove any matching words
        for i in range(len(split) - 1 , 0, -1):
            if splitSearchTerm.count(split[i]) > 0:
                splitSearchTerm.remove(split[i])
                split.remove(split[i])
        
        #join the remaining words
        remainingSearchTerm = ""
        remainingTokenName = ""
        for word in splitSearchTerm:
            remainingSearchTerm += word
        for word in split:
            remainingTokenName += word
            
        score = difflib.ndiff(remainingSearchTerm, remainingTokenName)
        diffList = list(score)
        if len(diffList) < bestDiffScore or bestDiffScore == -1:
            bestDiffScore = len(diffList)
            chosenTokenFile = token
                
    print(chosenTokenFile)

if __name__ == "__main__":
    main()