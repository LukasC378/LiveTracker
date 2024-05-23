export interface RaceData {
    driversData: DriverData[]
    lapCount: number
}

export interface DriverData {
    driverId: string,
    latitude: number,
    longitude: number,
    finished: boolean
}