import { Polygon, Tooltip } from 'react-leaflet';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { getDistrictsForMap } from '@/entities/district/model/districtApi';
import { useMapInteractionStore } from '@/features/create-marker/model/interactionStore';

export const DistrictsLayer = () => {
    const navigate = useNavigate();
    const { mode } = useMapInteractionStore();

    const { data: districts = [] } = useQuery({
        queryKey: ['districts-map'],
        queryFn: getDistrictsForMap,
        staleTime: Infinity,
    });

    const isInteracting = mode !== 'IDLE';
    const getDistrictColor = (id: string) => {
        const colors = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899'];
        const charSum = id.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
        return colors[charSum % colors.length];
    };
    return (
        <>
            {districts.map((d) => (
                <Polygon
                    key={d.id}
                    positions={d.geometry.map(c => [c.lat, c.lng])}
                    pathOptions={{
                        fillColor: getDistrictColor(d.id),
                        fillOpacity: isInteracting ? 0.05 : 0.12,
                        color: getDistrictColor(d.id),
                        weight: 1.5,
                        dashArray: '4'
                    }}
                    eventHandlers={{
                        click: (e) => {
                            if (isInteracting) return;
                            navigate(`/districts/${d.id}`);
                            e.originalEvent.stopPropagation();
                        },
                        mouseover: (e) => {
                            if (isInteracting) return;
                            const layer = e.target;
                            layer.setStyle({ fillOpacity: 0.2, weight: 2 });
                        },
                        mouseout: (e) => {
                            const layer = e.target;
                            layer.setStyle({ fillOpacity: 0.08, weight: 1 });
                        }
                    }}
                >
                    <Tooltip sticky>
                        <strong>Район {d.name}</strong>
                        <br/>
                        Кликните для просмотра статистики
                    </Tooltip>
                </Polygon>
            ))}
        </>
    );
};