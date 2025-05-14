import pandas as pd

CSV_FILEPATH = '../../Lab-2/datasource/plastics.csv'
COUNTRY_COLUMN_NAME = 'country'

df = pd.read_csv(CSV_FILEPATH)
unique_values = df[COUNTRY_COLUMN_NAME].unique()
comma_separated_list = ", ".join(str(value) for value in unique_values)

print(f"Unique values in column '{COUNTRY_COLUMN_NAME}': {comma_separated_list}")