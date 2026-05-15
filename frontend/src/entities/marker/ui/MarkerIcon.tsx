import L from 'leaflet';
import { getCategoryColor } from '../lib/getCategoryColor';
import './MarkerStyles.css';

export const createMarkerIcon = (category: string, status: string,  isOfficial?: boolean) => {
    const color = getCategoryColor(category);
    const isResolved = status === 'Resolved' || status === 'Passed' || status === 'Archived';
    const isPulsing = category === 'Project' || (category === 'Issue' && status === 'InProgress');
    const officialClass = isOfficial ? 'm-icon--official' : '';

    const classList = [
        'm-icon-body',
        isPulsing ? 'm-icon--pulsing' : '',
        isResolved ? 'm-icon--resolved' : ''
    ].join(' ');

    return L.divIcon({
        className: 'm-icon-container',
        html: `
          <div class="${classList} ${officialClass}" style="background-color: ${color};">
                ${isOfficial ? '🛡️' : (category === 'Project' ? '🏛' : '<div class="m-icon-dot"></div>')}
            </div>
        `,
        iconSize: [24, 24],
        iconAnchor: [12, 12],
    });
};