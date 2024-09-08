import requests
import csv
import json
from io import StringIO
import dataclasses
from dataclasses import dataclass

@dataclass
class RaceCurrency:
    id: int
    name: str

def get_racing_currencies():
    currency_types_csv = requests.get('https://wago.tools/db2/CurrencyTypes/csv').text
    buffer = StringIO(currency_types_csv)
    reader = csv.DictReader(buffer)

    for row in reader:
        id = int(row['ID'])
        category_id = int(row['CategoryID'])
        name = row['Name_lang']
        if category_id == 251:
            yield RaceCurrency(id, name)

def write_lua(racing_currencies):
    pass

def write_json(racing_currencies):
    class EnhancedJSONEncoder(json.JSONEncoder):
        def default(self, o):
            if dataclasses.is_dataclass(o):
                return dataclasses.asdict(o)
            return super().default(o)

    serialized = json.dumps(racing_currencies, cls=EnhancedJSONEncoder)
    with open("race_currencies.json", "w") as f:
        f.write(serialized)

racing_currencies = list(get_racing_currencies())
write_lua(racing_currencies)
write_json(racing_currencies)