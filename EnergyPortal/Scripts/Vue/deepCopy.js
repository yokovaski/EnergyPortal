const deepCopy = inObject => {
    let outObject, value, key;

    if(typeof inObject !== "object" || inObject === null) {
        // Return the value if inObject is not an object
        return inObject;
    }

    // Create an array or object to hold the values
    outObject = Array.isArray(inObject) ? [] : {};

    for (key in inObject) {
        value = inObject[key];

        // Recursively (deep) copy for nested objects, including arrays
        outObject[key] = (typeof value === "object" && value !== null) ? deepCopy(value) : value;
    }

    return outObject;
};

export default function (inObject) {
    return deepCopy(inObject);
}