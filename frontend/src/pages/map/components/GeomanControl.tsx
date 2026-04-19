import { useEffect } from 'react';
import { useMap } from 'react-leaflet';
import '@geoman-io/leaflet-geoman-free';
import '@geoman-io/leaflet-geoman-free/dist/leaflet-geoman.css';
import { useMapInteractionStore } from '@/features/create-marker/model/interactionStore';

export const GeomanControl = () => {
    const map = useMap();
    const { mode, addPoint } = useMapInteractionStore();

    useEffect(() => {
        map.pm.setLang('ru');

        map.on('pm:create', (e: any) => {
            const layer = e.layer;

            if (e.shape === 'Marker') {
                const { lat, lng } = layer.getLatLng();
                addPoint({ lat, lng });
            }

            if (e.shape === 'Polygon') {
                const coords = layer.getLatLngs()[0].map((c: any) => ({ lat: c.lat, lng: c.lng }));
                useMapInteractionStore.setState({ tempPoints: coords });
            }

            map.removeLayer(layer);
        });

        return () => {
            map.off('pm:create');
        };
    }, [map]);

    useEffect(() => {
        if (mode === 'IDLE') {
            map.pm.disableDraw();
            return;
        }

        if (mode === 'SELECT_POINT') {
            map.pm.enableDraw('Marker', { continueDrawing: false });
        }

        if (mode === 'DRAW_POLYGON') {
            map.pm.enableDraw('Polygon', {
                snappable: true,
                allowSelfIntersection: false,
                hintlineStyle: { color: 'blue', dashArray: '5,5' },
            });
        }
    }, [mode, map]);

    return null;
};