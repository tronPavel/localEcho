import L from 'leaflet';
import { getCategoryColor } from '../lib/getCategoryColor';
import './MarkerStyles.css';

/**
 * Создает иконку маркера.
 * Вся визуализация перенесена в CSS (.m-icon-container и т.д.)
 */
export const createMarkerIcon = (category: string, status: string) => {
    const color = getCategoryColor(category);
    const isResolved = status === 'Resolved' || status === 'Passed' || status === 'Archived';
    const isPulsing = category === 'Project' || (category === 'Issue' && status === 'InProgress');

    // Формируем список классов
    const classList = [
        'm-icon-body',
        isPulsing ? 'm-icon--pulsing' : '',
        isResolved ? 'm-icon--resolved' : ''
    ].join(' ');

    return L.divIcon({
        className: 'm-icon-container',
        html: `
            <div class="${classList}" style="background-color: ${color};">
                ${category === 'Project' ? '🏛' : '<div class="m-icon-dot"></div>'}
            </div>
        `,
        iconSize: [24, 24],
        iconAnchor: [12, 12],
    });
};