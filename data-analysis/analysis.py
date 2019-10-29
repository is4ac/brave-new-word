import csv
import json
from boardSolver import BoardSolver
import numpy as np

def load():
    with open("data/data.json") as json_file:
        data = json.load(json_file)

    return data

def extract_and_save_data():
    dictionary = {}

    # read in dictionary from file
    with open("data/sortedenddict.csv", mode="r") as csv_file:
        csv_reader = csv.reader(csv_file)
        for row in csv_reader:
            # only keep words that are 3 letters or longer
            if len(row[0]) >= 3:
                dictionary[row[0]] = float(row[1])

    # load data from json file
    data = load()

    # open csv file for writing analyzed data
    with open("data/analysis3.csv", "w") as analysis_file:
        # write header of file
        writer = csv.writer(analysis_file, delimiter=',', quotechar='"', quoting=csv.QUOTE_MINIMAL)
        writer.writerow(["UserID", "Word", "TimeTaken", "Success", "Rank", "MaxRank",
                         "Score", "MaxScore", "Rarity", "MaxRarity"])

        # analyze data
        for user in data:
            logs = data[user]
            #print(logs)
            start_time = logs[0]["timestampEpoch"]
            prestate_boards = [obj["payload"]["board"] for obj in logs if obj["key"] == "WF_GameState"
                               and obj["payload"]["state"] == "pre"]
            submits = [submit for submit in logs if submit["key"] == "WF_Submit"]
            payloads = [obj["payload"] for obj in submits]
            #print(payloads)
            submitted_words = [obj["word"] for obj in payloads]

            prev_time = start_time
            for index, word in enumerate(submitted_words):
                curr_time = submits[index]["timestampEpoch"]
                time_taken = curr_time - prev_time
                prev_time = curr_time
                bs = BoardSolver(prestate_boards[index], dictionary)
                print("Player ID: " + user)
                print("Word submitted: " + word)
                print("Time taken: " + str(time_taken))
                success = payloads[index]["success"]
                ranking = bs.get_ranking(word)
                max_rank = bs.get_length_all_words()
                max_score = bs.get_max_score()
                max_freq = bs.get_max_freq()
                score = bs.get_word_score(word)

                if word.lower() in dictionary:
                    freq = dictionary[word.lower()]
                else:
                    freq = 0

                writer.writerow([user, word, time_taken, success, ranking, max_rank, score, max_score, freq, max_freq])


def calculate_and_save_times():
    data = {}

    # read in dictionary from file
    with open("data/analysis.csv", mode="r") as csv_file:
        reader = csv.DictReader(csv_file)
        for row in reader:
            if row["UserID"] not in data:
                data[row["UserID"]] = []

            data[row["UserID"]].append(float(row["TimeTaken"]) / 1000)

    for user in data:
        print(user)
        print("Average: " + str(np.average(data[user])))
        print("Stdev: " + str(np.std(data[user])))

def calculate_success_attempts():
    data = {}

    # read in dictionary from file
    with open("data/analysis.csv", mode="r") as csv_file:
        reader = csv.DictReader(csv_file)
        for row in reader:
            if row["UserID"] not in data:
                data[row["UserID"]] = []

            data[row["UserID"]].append(row["Success"])

    for user in data:
        print(user)
        print("Total: " + str(len(data[user])))
        print("Num of unsuccessful attempts: " + str(len([x for x in data[user] if x == "False"])))

def calculate_scores():
    data = {}

    # read in dictionary from file
    with open("data/analysis.csv", mode="r") as csv_file:
        reader = csv.DictReader(csv_file)
        for row in reader:
            if row["UserID"] not in data:
                data[row["UserID"]] = []

            score = int(row["Score"])
            if score > 0:
                data[row["UserID"]].append(score)

    for user in data:
        print(user)
        print("Average: " + str(np.average(data[user])))
        print("Stdev: " + str(np.std(data[user])))

def calculate_rarity():
    data = {}

    # read in dictionary from file
    with open("data/analysis.csv", mode="r") as csv_file:
        reader = csv.DictReader(csv_file)
        for row in reader:
            if row["UserID"] not in data:
                data[row["UserID"]] = []

            rarity = float(row["Rarity"])
            if rarity > 0:
                data[row["UserID"]].append(rarity)

    for user in data:
        print(user)
        print("Average: " + str(np.average(data[user])))
        print("Stdev: " + str(np.std(data[user])))

def calculate_power():
    data = {}

    # read in dictionary from file
    with open("data/analysis.csv", mode="r") as csv_file:
        reader = csv.DictReader(csv_file)
        for row in reader:
            if row["UserID"] not in data:
                data[row["UserID"]] = []

            a = 0.25
            b = 0.25
            c = 0.5
            score = float(row["Score"])
            max_score = float(row["MaxScore"])
            rarity = float(row["Rarity"])
            max_rarity = float(row["MaxRarity"])
            rank = float(row["Rank"])
            max_rank = float(row["MaxRank"])
            if score > 0:
                data[row["UserID"]].append(a * (score / max_score) +
                                           b * (rarity / max_rarity) +
                                           c * (max_rank - rank / (max_rank - 1)))

            #print(data)


    for user in data:
        print(user)
        print("Average: " + str(np.average(data[user])))
        print("Stdev: " + str(np.std(data[user])))


if __name__ == '__main__':
    #extract_and_save_data()

    # calculate times
    #calculate_and_save_times()

    # calculate unsuccessful vs successful attempts
    #calculate_success_attempts()

    calculate_power()