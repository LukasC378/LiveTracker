export interface Driver{
    id: number
    name: string,
    surname: string,
    number: number,
    teamId?: number,
    teamName?: string,
    color: string,
    gpsDevice: string
}