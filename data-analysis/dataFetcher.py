from firebase import firebase
import json

class DataFetcher:
    """Class that gets data from Firebase and processes it to a certain degree"""

    def __init__(self,
                 url='https://wordflood-bf7c4.firebaseio.com/',
                 log='/WFLogs_V1_0_0'):
        authentication = firebase.FirebaseAuthentication('PWHiTWREptncVLs9i1KQMyIAESF66u4STEQYxuIq',
                                                         'isaacsung@gmail.com')
        self.fb = firebase.FirebaseApplication(url, authentication=authentication)
        self.log = log
        self.log_games = log + '_games'

    @staticmethod
    def get_perm_val(button, score, highlight, tutorial):
        val = 0

        if tutorial:
            val += 1
        if highlight:
            val += 2
        if score:
            val += 4
        if button:
            val += 8

        return val

    def get_permutations(self):
        result = self.fb.get(self.log_games, None)

        # set up the permutation dict
        perms = {}
        for i in range(16):
            perms[str(i)] = 0

        # count how many of each game type there are and store it in the permuation dictionary
        for key, value in result.items():
            perm_num = self.get_perm_val(value['displayButton'],
                                         value['displaySelectedScore'],
                                         value['displayHighlightFeedback'],
                                         value['displayTutorial'])
            perms[str(perm_num)] += 1

        return perms

    def get_all_userIDs(self):
        userIDs = []

        result = self.fb.get(self.log_games, None)
        for key, value in result.items():
            userIDs.append(value['userID'])

        return userIDs

    def get_all_actions_by_userID(self, userID=0):
        actions = []

        result = self.fb.get(self.log, None)
        for key, value in result.items():
            if value['userID'] == userID:
                actions.append(value)

        return actions


def main():
    fetcher = DataFetcher()
    perms = fetcher.get_permutations()
    print(perms)
    userIDs = fetcher.get_all_userIDs()
    print(userIDs)
    userActions = {}

    for userID in userIDs:
        userActions[userID] = fetcher.get_all_actions_by_userID(userID=userID)

    # write data to json files
    json_data = json.dumps(userActions)
    f = open("data/data.json", "w")
    f.write(json_data)
    f.close()


if __name__ == "__main__":
    main()
