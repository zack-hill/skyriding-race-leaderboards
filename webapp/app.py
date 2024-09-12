from flask import Flask, request, render_template
from flask_bootstrap import Bootstrap5
from storage import Storage

app = Flask(__name__)
bootstrap = Bootstrap5(app)


@app.route("/")
def index():
    storage = Storage()
    race_times = storage.get_all_races()
    return render_template("index.html", race_times=race_times)


@app.route("/race_leaderboard", methods=["GET"])
def race_leaderboard():
    race_id = request.args.get("race_id")
    storage = Storage()
    race_times = storage.get_race_times(race_id)
    return render_template("leaderboard.html", race_id=race_id, race_times=race_times)


@app.route("/upload", methods=["POST"])
def upload_data():
    storage = Storage()
    result = request.get_json()
    battle_tag = result["BattleTag"]
    for char_race_data in result["CharacterRaceData"]:
        character_name = char_race_data["CharacterName"]
        for race_time in char_race_data["RaceTimes"]:
            race_id = race_time["RaceId"]
            time_ms = race_time["TimeMs"]
            storage.update_time(battle_tag, race_id, int(time_ms), character_name)
    storage.commit()
    return "", 200
