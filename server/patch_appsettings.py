import json
import sys

pathToJsonCredentials = sys.argv[1]

with open(pathToJsonCredentials, encoding='utf-8') as f:
    data = json.load(f)
    username = data["ElasticUsername"]
    password = data["ElasticPassword"]

with open('appsettings.json', encoding='utf-8') as f:
    data = json.load(f)

with open('appsettings.json', 'w', encoding='utf-8') as f:
    data["ElasticConfiguration"]["Username"] = username
    data["ElasticConfiguration"]["Password"] = password
    json.dump(data, f, ensure_ascii=False, indent=4)
