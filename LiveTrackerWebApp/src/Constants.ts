import {User} from "./Models/User";
import {GeoJsonObject} from "geojson";
import {Collection} from "./Models/Collection";
import {SessionStateFilterEnum, UserRoleEnum} from "./Models/Enums";
import {Layout} from "./Models/Layout";
import {Session, SessionFilerUI, SessionResultVM} from "./Models/Session";

export class Constants{
    public static get unexpectedErrorMessage(): string {
        return "An unexpected error occurred. Please try again later.";
    }

    public static get defaultUser(): User {
        return {
            username: "", email: "", role: UserRoleEnum.NormalUser
        }
    }

    public static get defaultGeoJson(): GeoJsonObject {
        return {
            type: "Feature"
        }
    };

    public static get defaultColor(): string {
        return "#000000";
    }

    public static get defaultCollection(): Collection {
        return {
            id: 0,
            name: "",
            drivers: [],
            teams: [],
            useTeams: false
        }
    }

    public static get defaultLayout(): Layout {
        return {
            id: 0,
            name: "",
            geoJson: ""
        }
    }

    public static get defaultSession(): Session {
        return {
            id: 0,
            name: "",
            collectionId: 0,
            geoJson: "",
            laps: 0,
            scheduledFrom: "",
            scheduledTo: ""
        }
    }

    public static get defaultSessionFilterUI(): SessionFilerUI {
        return {
            date: "",
            orderAsc: true,
            searchTerm: "",
            organizerId: 0,
            sessionState: SessionStateFilterEnum.Upcoming
        }
    }

    public static get defaultSessionResult(): SessionResultVM {
        return {
            drivers: [], id: 0, name: ""
        }
    }
}