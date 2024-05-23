import {UserContext} from "../App";
import {validate, ValidationResult} from "geojsonjs";
import L from "leaflet";

export class Utils{

    public static resultPasswordProgressBarPercentage(password: string): number{
        let numberOfCompletedConditions = 0

        if(password.length >= 8){
            numberOfCompletedConditions += 1
        }
        if(/^(?=.*[A-Z]).*$/.test(password)){
            numberOfCompletedConditions += 1
        }
        if(/[0-9]/.test(password)){
            numberOfCompletedConditions += 1
        }
        if(/^(?=.*[~`!@#$%^&*()--+={}\[\]|\\:;"'<>,.?/_₹]).*$/.test(password)){
            numberOfCompletedConditions += 1
        }

        return 25 * numberOfCompletedConditions
    }

    public static resultPasswordProgressBarVariant(password: string): string{
        let progressBarPercentage = Utils.resultPasswordProgressBarPercentage(password)

        if(progressBarPercentage <= 25){
            return "danger"
        }
        else if(progressBarPercentage <= 50){
            return "warning"
        }
        else if(progressBarPercentage <= 75){
            return "info"
        }
        else if(progressBarPercentage == 100){
            return "success"
        }
        else{
            return "danger"
        }
    }

    public static findDuplicateStrings(strings: string[]): string[] {
        const numberCount: { [key: string]: number } = {};
        const duplicates: string[] = [];

        for (const str of strings) {
            numberCount[str] = (numberCount[str] || 0) + 1;
        }

        for (const str in numberCount) {
            if (numberCount.hasOwnProperty(str) && numberCount[str] > 1) {
                duplicates.push(str);
            }
        }

        return duplicates;
    }

    public static findDuplicateNumbers(numbers: number[]): number[] {
        const numberCount: { [key: number]: number } = {};
        const duplicates: number[] = [];

        for (const number of numbers) {
            numberCount[number] = (numberCount[number] || 0) + 1;
        }

        for (const number in numberCount) {
            if (numberCount.hasOwnProperty(number) && numberCount[number] > 1) {
                duplicates.push(Number(number));
            }
        }

        return duplicates;
    }

    public static userIsLogged(user: typeof UserContext): boolean{
        return !this.isNullOrEmpty(user.email) && !this.isNullOrEmpty(user.username);
    }

    public static isNullOrEmpty(input?: string): boolean{
        return input == null || false || input === ""
    }

    public static validateGeoJson(geoJsonString: string): [boolean, any, string] {
        let message: string = "";

        try{
            const geoJson = JSON.parse(geoJsonString);
            const validationResult: ValidationResult = validate(geoJson);
            let result: boolean = validationResult.valid

            let geometry = geoJson.features[0]?.geometry;
            if(!geometry || geometry.type !== 'LineString' || geoJson.features.length > 1){
                result = false;
                message = "GeoJson not matching rules"
            }

            return [result, geoJson, message]
        }
        catch (err){
            message = "GeoJson is not valid"
            return [false, null, message]
        }
    }

    public static showClusterTooltip(e: L.LeafletMouseEvent){
        let text = ''
        const cluster = e.propagatedFrom as L.MarkerCluster;
        console.log(cluster.getAllChildMarkers())
        cluster.getAllChildMarkers()
            .reverse()
            .forEach(x => {
                const tooltip = (x.getTooltip()!.options as any).children.props.children.props.children as string
                text += tooltip + '<br>'
            })
        e.propagatedFrom.bindTooltip(text).openTooltip();
    }
}