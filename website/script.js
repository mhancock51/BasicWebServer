let count = 0;

function LogHelloWorld() {
    console.log("Hello world!");
    count++;
    document.getElementById("count-display").innerHTML = "Button clicked " + count.toString() + " times";
}