import { useState, useEffect } from 'react';

export const useLoadingDelay = (loading: boolean, delay: number = 400) => {
    const [show, setShow] = useState(false);

    useEffect(() => {
        let timer: any;
        if (loading) {
            timer = setTimeout(() => setShow(true), delay);
        } else {
            setShow(false);
        }
        return () => clearTimeout(timer);
    }, [loading, delay]);

    return show;
};