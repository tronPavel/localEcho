import { useState } from 'react';
import { Swiper, SwiperSlide } from 'swiper/react';
import { Navigation, Pagination } from 'swiper/modules';
import { getImageUrl } from "@/shared/api/apiInstance";
import { classNames } from "@/shared/lib/utils/classNames";

//@ts-ignore
import 'swiper/css';
//@ts-ignore
import 'swiper/css/navigation';
//@ts-ignore
import 'swiper/css/pagination';

import styles from './ImageSlider.module.css';

interface ImageSliderProps {
    urls: string[];
    height?: number | string;
}


const SmartImage = ({ url }: { url: string }) => {
    const [isLoaded, setIsLoaded] = useState(false);

    return (
        <div className={styles.slideWrapper}>
            {!isLoaded && (
                <div className={styles.loader}>
                    <div className={styles.spinner} />
                    <span className={styles.loaderText}>Загрузка...</span>
                </div>
            )}
            <img
                src={getImageUrl(url)}
                alt="slide content"
                className={classNames(styles.image, isLoaded && styles.imageLoaded)}
                onLoad={() => setIsLoaded(true)}
                loading="lazy"
            />
        </div>
    );
};

export const ImageSlider = ({ urls, height = 320 }: ImageSliderProps) => {
    if (!urls || urls.length === 0) {
        return <div className={styles.noImage} style={{ height }}>Фото отсутствует</div>;
    }

    return (
        <Swiper
            modules={[Navigation, Pagination]}
            navigation={urls.length > 1}
            pagination={{ clickable: true }}
            className={styles.slider}
            style={{ height }}
        >
            {urls.map((url, i) => (
                <SwiperSlide key={url || i}>
                    <SmartImage url={url} />
                </SwiperSlide>
            ))}
        </Swiper>
    );
};