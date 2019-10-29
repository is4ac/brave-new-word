from wordScorer import WordScorer
from operator import itemgetter

class BoardSolver:
    def __init__(self, board='', dictionary={}):
        self.board = self.convert_string_to_board_list(board)
        self.dictionary = dictionary
        self.scorer = WordScorer(dictionary)
        self.N = len(self.board)  # number of rows
        self.M = len(self.board[0])  # number of cols
        self.MAX = 45  # max length of word

        # direction X-Y delta pairs for adjacent cells
        self.dx = [0, 1, 1, 1, 0, -1, -1, -1]
        self.dy = [1, 1, 0, -1, -1, -1, 0, 1]

        # dynamic programming table
        self.dp = [[[False for x in range(self.M)] for y in range(self.N)] for z in range(self.MAX)]

        # table to keep track of visited spaces
        self.visited = [[False for x in range(self.M)] for y in range(self.N)]
        self.words = []
        self.scores = []
        self.freqs = []

    @staticmethod
    def convert_string_to_board_list(boardStr):
        """
        Converts a string like "ABCDEF\nGHIJKL\nMNOPQR\n..." to a 2D list like so:
        [["A", "B", "C", "D", "E", "F"], ["G", "H", ...], ... ]
        :param boardStr:
        :return:
        """
        board = []
        rows = boardStr.split("\n")

        for row in rows:
            rowList = []
            for letter in row:
                rowList.append(letter)
            board.append(rowList)

        return board

    def check_board(self, word, curIndex, row, col):
        if curIndex == len(word) - 1:
            return True

        ret = False;

        for i in range(8):
            newR = row + self.dx[i]
            newC = col + self.dy[i]

            if (0 <= newR < self.N and 0 <= newC < self.M
                and not self.visited[newR][newC]
                    and word[curIndex+1] == self.board[newR][newC]):
                curIndex += 1
                self.visited[newR][newC] = True

                ret = self.check_board(word, curIndex, newR, newC)
                if ret:
                    break

                curIndex -= 1
                self.visited[newR][newC] = False

        return ret

    def get_all_words(self):
        """
        Returns a list of all the words possible on the current board
        :return:
        """
        all_words = []
        prev = ""

        # loop through all the words in the dictionary
        for word in self.dictionary:
            word = word.upper()
            i = 0
            if len(prev) < len(word):
                length = len(prev)
            else:
                length = len(word)

            for i in range(length):
                if prev[i] != word[i]:
                    break

            # slight optimization to check for overlapping parts of words
            first_mismatch = i

            # initialize the base case
            if first_mismatch == 0:
                for i in range(self.N):
                    for j in range(self.M):
                        if self.board[i][j] == word[0]:
                            self.dp[0][i][j] = True
                        else:
                            self.dp[0][i][j] = False
                first_mismatch = 1

            # loop through and check the board for each letter of the word
            for k in range(first_mismatch, len(word)):
                for i in range(self.N):
                    for j in range(self.M):
                        self.dp[k][i][j] = False

                        if self.board[i][j] != word[k]:
                            continue

                        l = 0
                        while l < 8 and not self.dp[k][i][j]:
                            ti = i + self.dx[l]
                            tj = j + self.dy[l]
                            l += 1

                            if ti < 0 or ti >= self.N or tj < 0 or tj >= self.M:
                                continue

                            if self.dp[k - 1][ti][tj]:
                                self.dp[k][i][j] = True

            # check to see if the word exists on the board according to the dp table
            flag = False
            for i in range(self.N):
                if flag:
                    break

                for j in range(self.M):
                    if self.dp[len(word)-1][i][j]:
                        flag = True
                        break

            # dp table says word exists, but double check and make sure that you don't visit a cell twice
            if flag:
                verified = False

                for i in range(self.N):
                    if verified:
                        break

                    for j in range(self.M):
                        if word[0] != self.board[i][j]:
                            continue

                        # reset the visited board
                        self.visited = [[False for x in range(self.M)] for y in range(self.N)]
                        self.visited[i][j] = True

                        if self.check_board(word, 0, i, j):
                            all_words.append(word)
                            verified = True
                            break

            prev = word

        self.words = all_words
        return all_words

    def get_max_score(self):
        if len(self.words) == 0:
            self.get_all_words()

        self.scores = []

        max_score = 0

        for word in self.words:
            score = self.scorer.getScore(word)
            #print(score)
            self.scores.append(score)
            if word.lower() in self.dictionary:
                freq = self.dictionary[word.lower()]
            else:
                freq = 0
            if score > max_score and freq != 1.0:
                max_score = score

        return max_score

    def get_max_freq(self):
        if len(self.words) == 0:
            self.get_all_words()

        self.freqs = []

        max_freq = 0

        for word in self.words:
            if word.lower() in self.dictionary:
                freq = self.dictionary[word.lower()]
            else:
                freq = 0
            #print(freq)
            self.freqs.append(freq)
            if freq > max_freq and freq != 1.0:
                max_freq = freq

        return max_freq

    def get_ranking(self, word):
        word = word.upper()

        if len(self.words) == 0:
            self.get_all_words()

        if len(self.scores) == 0:
            self.get_max_score()

        ranked_words = [x for _, x in sorted(zip(self.scores, self.words), reverse=True)]

        for index, ranked in enumerate(ranked_words, start=1):
            if word == ranked:
                return index

        return -1

    def get_length_all_words(self):
        if len(self.words) == 0:
            self.get_all_words()

        return len(self.words)

    def get_word_score(self, word):
        return self.scorer.getScore(word)

"""
boardSolver = BoardSolver(board="ABCDEF\nGHIJKL\nMNOPQR\nSTUVWX\nYZABCD")
print(boardSolver.board[0][1])
"""