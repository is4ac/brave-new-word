import pandas as pd 


filename = './data/sortedenddict.csv'


def main():
    df = pd.read_csv(filename, header=None, names=['word', 'freq'])
    df['pct_rank'] = df['freq'].rank(pct=True)
    print(df.head())

    # WARNING: this skips the literal word "null" -- you'll have to find it and replace it in the file.
    df.round(3).to_csv('./data/sorted_dict_ranks.csv', index=False)


def summary():
    df = pd.read_csv('./data/sorted_dict_ranks.csv')
    unique = df['pct_rank'].unique()
    unique.sort()
    print(unique)


if __name__ == '__main__':
    #main()
    summary()