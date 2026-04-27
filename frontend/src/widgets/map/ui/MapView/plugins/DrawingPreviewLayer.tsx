import { Marker, Polygon, CircleMarker } from 'react-leaflet';
import { useMapInteractionStore } from '@/features/map-control/model/interactionStore.ts';

export const DrawingPreviewLayer = () => {
    const { tempPoints, mode } = useMapInteractionStore();

    if (tempPoints.length === 0) return null;

    return (
        <>
            {mode === 'SELECT_POINT' && (
                <Marker position={[tempPoints[0].lat, tempPoints[0].lng]} />
            )}

            {mode === 'DRAW_POLYGON' && (
                <>
                    {tempPoints.map((p, i) => (
                        <CircleMarker
                            key={i}
                            center={[p.lat, p.lng]}
                            radius={4}
                            pathOptions={{ color: 'blue', fillOpacity: 1 }}
                        />
                    ))}
                    {tempPoints.length >= 2 && (
                        <Polygon
                            positions={tempPoints.map(p => [p.lat, p.lng])}
                            pathOptions={{ color: '#3b82f6', dashArray: '10, 10', fillOpacity: 0.1 }}
                        />
                    )}
                </>
            )}
        </>
    );
};