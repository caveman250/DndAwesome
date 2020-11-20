from os import listdir
from os.path import isfile, join
import re
import difflib
import json

def main():
    all_token_files = listdir("../images/dnd_tokens")
    token_paths = []
    for token in all_token_files:
        #token = join("images/dnd_tokens/", token)
        token = token[:-4]
        token_paths.append(token)
    print(token_paths)

    with open('../data/database/monsters.json') as f:
        monster_database = json.load(f)
    
    chosen_tokens = dict()
    index = 0
    for monster in monster_database:
        search_term = monster["name"]
        bestDiffScore = -1
        chosenTokenFile = ""
        for token in token_paths:
            split = re.findall('[A-Z][^A-Z]*', token)
            splitSearchTerm = search_term.split(' ')
            
            num_matching_words = 0
            #remove any matching words
            for i in range(len(splitSearchTerm) -1, -1, -1):
                if split.count(splitSearchTerm[i]) > 0:
                    split.remove(splitSearchTerm[i])
                    splitSearchTerm.remove(splitSearchTerm[i])
                    num_matching_words += 1
            
            if num_matching_words == 0:
                continue
            
            #join the remaining words
            remainingSearchTerm = ""
            remainingTokenName = ""
            for word in splitSearchTerm:
                remainingSearchTerm += word
            for word in split:
                remainingTokenName += word

            if len(remainingTokenName) != 0 and len(remainingSearchTerm) != 0:
                continue
                
            diffs = difflib.ndiff(remainingSearchTerm, remainingTokenName)
            diffList = list(diffs)
            
            matching_word_multiplier = 50
            
            score = len(diffList) * 5 - matching_word_multiplier * num_matching_words
            if score < bestDiffScore or bestDiffScore == -1:
                bestDiffScore = score
                chosenTokenFile = token
        print("Search term: " + search_term)
        print("Chosen Token: " + chosenTokenFile)
        chosen_tokens[index] = chosenTokenFile
        index+=1
    
    for key in chosen_tokens:
        monster_database[key]["image"] = "images/dnd_tokens/" + chosen_tokens[key] + ".png"
    
    filehandle = open('../data/database/monsters.json', 'w')
    filehandle.write(json.dumps(monster_database))
    filehandle.close()


if __name__ == "__main__":
    main()