import { Swiper, SwiperSlide } from 'swiper/react';
import { Navigation, Pagination } from 'swiper/modules';
import { getImageUrl } from "@/shared/api/apiInstance";

import 'swiper/css';
import 'swiper/css/navigation';
import 'swiper/css/pagination';
import styles from './ImageSlider.module.css';

interface ImageSliderProps {
    urls: string[];
    height?: number | string;
}

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
                    <img
                        src={getImageUrl(url)}
                        alt="slide"
                        className={styles.image}
                        loading="lazy"
                    />
                </SwiperSlide>
            ))}
        </Swiper>
    );
};