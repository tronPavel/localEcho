import L from 'leaflet';
import { getCategoryColor } from '../lib/getCategoryColor';

export const createMarkerIcon = (category: string) => {
    const isOfficial = category === 'Project'; // Временно определяем по категории
    const color = getCategoryColor(category);

    const className = isOfficial ? 'custom-marker custom-marker-pulsing' : 'custom-marker';

    return L.divIcon({
        className: className,
        html: `
            <div style="
                background-color: ${color}; 
                width: 24px; 
                height: 24px; 
                border-radius: 50%; 
                border: 2px solid white;
                display: flex;
                align-items: center;
                justify-content: center;
                box-shadow: 0 2px 5px rgba(0,0,0,0.2);
                color: white;
                font-size: 10px;
                font-weight: bold;
            ">
                ${isOfficial ? '🏛' : ''} 
            </div>`,
        iconSize: [24, 24],
        iconAnchor: [12, 12],
    });
};