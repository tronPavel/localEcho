import { useEffect } from 'react';
import { useMap } from 'react-leaflet';
import { GeoSearchControl, OpenStreetMapProvider } from 'leaflet-geosearch';
import 'leaflet-geosearch/dist/geosearch.css';
import './SearchControl.module.css';

export const SearchControl = () => {
    const map = useMap();

    useEffect(() => {
        const provider = new OpenStreetMapProvider({
            params: {
                'accept-language': 'ru',
                countrycodes: 'by',
            },
        });

        // @ts-ignore
        const searchControl = new GeoSearchControl({
            provider,
            style: 'bar',
            showMarker: true,
            showPopup: false,
            autoClose: true,
            retainZoomLevel: false,
            animateZoom: true,
            keepResult: true,
            searchLabel: 'Введите адрес в Минске...',
        });

        map.addControl(searchControl);
        return () => { map.removeControl(searchControl); };
    }, [map]);

    return null;
};