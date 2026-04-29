const addWorkoutBtn = document.getElementById("addWorkoutBtn");
const workoutTable = document.getElementById("workoutTable");
const progressBar = document.querySelector(".progress");
const progressText = document.getElementById("progressText");
const editStatsBtn = document.getElementById("editStatsBtn");

const weightValue = document.getElementById("weightValue");
const goalWeightValue = document.getElementById("goalWeightValue");
const bmiValue = document.getElementById("bmiValue");
const prValue = document.getElementById("prValue");
const goalBenchValue = document.getElementById("goalBenchValue");

let workouts = JSON.parse(localStorage.getItem("workouts")) || [];

let stats = JSON.parse(localStorage.getItem("stats")) || {
  weight: 77,
  goalWeight: 82,
  bmi: 24,
  latestPR: "Bench 110 kg",
  currentBench: 110,
  goalBench: 120
};

function saveStats() {
  localStorage.setItem("stats", JSON.stringify(stats));
}

function displayStats() {
  weightValue.textContent = stats.weight;
  goalWeightValue.textContent = stats.goalWeight;
  bmiValue.textContent = stats.bmi;
  prValue.textContent = stats.latestPR;
  goalBenchValue.textContent = stats.goalBench;
}

function updateProgress() {
  let percentage = (stats.currentBench / stats.goalBench) * 100;

  if (percentage > 100) percentage = 100;

  progressBar.style.width = percentage + "%";
  progressText.textContent = "Progress: " + Math.round(percentage) + "%";
}

function saveWorkouts() {
  localStorage.setItem("workouts", JSON.stringify(workouts));
}

const workoutList = document.getElementById("workoutList");

function addWorkoutToList(workout) {
  const row = document.createElement("div");
  row.classList.add("workout-row");

  row.innerHTML = `
    <p>${workout.date}</p>
    <p>${workout.exercise}</p>
    <p>${workout.weight} kg</p>
    <p>${workout.reps}</p>
  `;

  workoutList.appendChild(row);
}

function loadWorkouts() {
  workouts.forEach(workout => {
    addWorkoutToList(workout);
  });
}

addWorkoutBtn.addEventListener("click", () => {
  const date = prompt("Enter date:");
  const exercise = prompt("Enter exercise:");
  const weight = prompt("Enter weight:");
  const reps = prompt("Enter reps:");

  if (date && exercise && weight && reps) {
    const workout = { date, exercise, weight, reps };

    workouts.push(workout);
    saveWorkouts();
    addWorkoutToList(workout);

    if (exercise.toLowerCase() === "bench press") {
      if (parseFloat(weight) > stats.currentBench) {
        stats.currentBench = parseFloat(weight);
        stats.latestPR = "Bench " + weight + " kg";
        saveStats();
        displayStats();
        updateProgress();
      }
    }
  }
});

editStatsBtn.addEventListener("click", () => {
  stats.weight = prompt("Current weight:", stats.weight) || stats.weight;
  stats.goalWeight = prompt("Goal weight:", stats.goalWeight) || stats.goalWeight;
  stats.bmi = prompt("BMI:", stats.bmi) || stats.bmi;
  stats.goalBench = prompt("Bench goal:", stats.goalBench) || stats.goalBench;

  saveStats();
  displayStats();
  updateProgress();
});

loadWorkouts();
displayStats();
updateProgress();