document.getElementById("nah")
const student = {Name: "Nahom" Age: "16"}
let score = 50;
let grade;
if (score >= 90) {
  grade = "A";
} 
if (score >= 60 ){
grade= "Pass"
}
 else {
  grade = "Try again";
}
console.log("grade:" + grade)
console.log("score: " + score);