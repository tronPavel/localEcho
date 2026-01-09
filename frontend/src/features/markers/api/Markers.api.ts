import axios from 'axios';
import type {MarkerDto} from "../types/MarkerDto.ts";
import type {CreateMarkerDto} from "../types/CreateMarkerDto.ts";

const api = axios.create({
    baseURL: 'http://localhost:5015',
});

export const getMarkers = async (): Promise<MarkerDto[]> => {
    const response = await api.get('/api/markers');
    return response.data;
};

export const createMarker = async (data : CreateMarkerDto)=>{
    await api.post('/api/markers', data)
}