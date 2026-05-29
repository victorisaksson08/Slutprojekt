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

  const dateCell = document.createElement("p");
  dateCell.textContent = workout.date;

  const exerciseCell = document.createElement("p");
  exerciseCell.textContent = workout.exercise;

  const weightCell = document.createElement("p");
  weightCell.textContent = workout.weight + " kg";

  const repsCell = document.createElement("p");
  repsCell.textContent = workout.reps;

  row.appendChild(dateCell);
  row.appendChild(exerciseCell);
  row.appendChild(weightCell);
  row.appendChild(repsCell);

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
  const weightInput = prompt("Enter weight:");
  const repsInput = prompt("Enter reps:");

  if (!date || !exercise || !weightInput || !repsInput) return;

  const weight = parseFloat(weightInput);
  const reps = parseInt(repsInput, 10);

  if (isNaN(weight) || weight <= 0) {
    alert("Weight must be a positive number.");
    return;
  }
  if (isNaN(reps) || reps <= 0) {
    alert("Reps must be a positive whole number.");
    return;
  }

  const workout = { date, exercise, weight, reps };

  workouts.push(workout);
  saveWorkouts();
  addWorkoutToList(workout);

  if (exercise.toLowerCase() === "bench press") {
    if (weight > stats.currentBench) {
      stats.currentBench = weight;
      stats.latestPR = "Bench " + weight + " kg";
      saveStats();
      displayStats();
      updateProgress();
    }
  }
});

editStatsBtn.addEventListener("click", () => {
  const weightInput = prompt("Current weight:", stats.weight);
  const goalWeightInput = prompt("Goal weight:", stats.goalWeight);
  const bmiInput = prompt("BMI:", stats.bmi);
  const goalBenchInput = prompt("Bench goal:", stats.goalBench);

  if (weightInput !== null && weightInput !== "") {
    const v = parseFloat(weightInput);
    if (!isNaN(v) && v > 0) stats.weight = v;
  }
  if (goalWeightInput !== null && goalWeightInput !== "") {
    const v = parseFloat(goalWeightInput);
    if (!isNaN(v) && v > 0) stats.goalWeight = v;
  }
  if (bmiInput !== null && bmiInput !== "") {
    const v = parseFloat(bmiInput);
    if (!isNaN(v) && v > 0) stats.bmi = v;
  }
  if (goalBenchInput !== null && goalBenchInput !== "") {
    const v = parseFloat(goalBenchInput);
    if (!isNaN(v) && v > 0) stats.goalBench = v;
  }

  saveStats();
  displayStats();
  updateProgress();
});

loadWorkouts();
displayStats();
updateProgress();