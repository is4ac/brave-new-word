class WordScorer:
    def __init__(self, dictionary={}):
        self.dictionary = dictionary

    def getScore(self, word):
        word = word.lower()

        # scoring function based on freq of word + freq of letters
        # TODO: do more balance testing of scoring function to make sure it is balanced?
        wordFreq = self.getWordFreq(word)

        if wordFreq < 0:
            # word does not exist in dictionary
            return 0

        # base score is based on length of word (if word is 3 letters long, base is 1 + 2 + 3 points, etc)
        baseScore = self.calculateBaseScore(len(word))

        # freq multiplier to reward rarer words
        freqMultiplied = wordFreq
        freq = 0.1
        while freq < 0.65:
            if wordFreq > freq:
                freqMultiplied *= 1.2
            freq += 0.075

        # TODO: based on the rarity of the word, Uncommon, Rare, Super Rare, Ultra Rare?
        # give the user a fixed bonus points amount and display an animation
        bonus = self.getBonus(wordFreq)

        return int((baseScore * (freqMultiplied * 20)) + bonus)

    def getWordFreq(self, word):
        if word in self.dictionary:
            return self.dictionary[word]
        else:
            return 0

    def getBonus(self, freq):
        if freq >= 0.38:
            # PREMIUM ULTRA RARE
            return 75 # over 9000
        elif freq >= 0.3:
            # PREMIUM ULTRA RARE
            return 60
        elif freq >= 0.24:
            # ULTRA RARE+
            return 50
        elif freq >= 0.2:
            # ULTRA RARE
            return 40
        elif freq >= 0.15:
            # SUPER RARE
            return 30
        elif freq >= 0.13:
            # RARE
            return 20
        elif freq >= 0.11:
            # AVERAGE
            return 10

        return 0

    def calculateBaseScore(self, length):
        if length == 0:
            return 0

        return self.lengthScoreFunction(length) + self.calculateBaseScore(length - 1)

    def lengthScoreFunction(self, length):
        if length == 1:
            return 1

        return 1 + self.lengthScoreFunction(length - 1)