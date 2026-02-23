import L from 'leaflet';
import { getCategoryColor } from '../lib/getCategoryColor';

export const createMarkerIcon = (category: string) => {
    return L.divIcon({
        className: 'custom-marker',
        html: `<div style="background-color: ${getCategoryColor(category)}; width: 20px; height: 20px; border-radius: 50%; border: 2px solid white;"></div>`,
        iconSize: [20, 20],
        iconAnchor: [10, 10],
    });
};