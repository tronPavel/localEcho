import type {Coordinate} from "@/entities/marker";

export interface CityBriefDto {
    id: string;
    name: string;
    lat: number;
    lng: number;
}

export interface CityMapDto {
    id: string;
    name: string;
    geometry: Coordinate[];
}