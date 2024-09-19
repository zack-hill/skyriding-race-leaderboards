from flask import Flask, request, render_template
from flask_bootstrap import Bootstrap5
from storage import Storage

app = Flask(__name__)
bootstrap = Bootstrap5(app)

storage = Storage()
all_race_info = storage.get_all_race_info()


@app.route("/")
def index():
    storage = Storage()
    race_times = storage.get_all_race_times()
    format_race_times(race_times)
    return render_template("index.html", race_times=race_times)


@app.route("/race_leaderboard", methods=["GET"])
def race_leaderboard():
    race_id = request.args.get("race_id")
    storage = Storage()
    race_times = storage.get_race_times(race_id)
    format_race_times(race_times)
    race_name = race_id
    race_info = all_race_info.get(race_id)
    if race_info is not None:
        race_name = race_info.name
        print(race_name)
    return render_template(
        "leaderboard.html", race_name=race_name, race_times=race_times
    )


@app.route("/upload", methods=["POST"])
def upload_data():
    storage = Storage()
    result = request.get_json()
    print(result)
    battle_tag = result["battleTag"]
    for char_race_data in result["characterRaceData"]:
        character_name = char_race_data["characterName"]
        for race_time in char_race_data["raceTimes"]:
            race_id = race_time["raceId"]
            time_ms = race_time["timeMs"]
            storage.update_time(battle_tag, race_id, int(time_ms), character_name)
    storage.commit()
    return "", 200


def format_time(time_ms) -> str:
    time_s = time_ms / 1000
    hours, rem = divmod(time_s, 3600)
    minutes, seconds = divmod(rem, 60)
    result_str = ""
    if hours > 0:
        result_str += f"{int(hours)}:"
    if minutes > 0:
        result_str += f"{int(minutes)}:"
    result_str += f"{round(seconds, 3):.3f}"
    return result_str


def format_race_times(race_times):
    for race_time in race_times:
        time_disp = format_time(race_time.time_ms)
        race_time.time_disp = time_disp
        race_info = all_race_info.get(race_time.race_id)
        race_time.race_name = race_time.race_id
        if race_info is not None:
            race_time.race_name = race_info.name
