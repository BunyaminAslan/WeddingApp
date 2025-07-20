document.addEventListener('DOMContentLoaded', () => {
    // --- Arka Plan Slider Fonksiyonelliği ---
    const slides = document.querySelectorAll('.background-slider .slide');
    let currentSlide = 0;

    function nextSlide() {
        // Mevcut slide'ı gizle
        slides[currentSlide].classList.remove('active');
        const currentVideo = slides[currentSlide].querySelector('video');
        if (currentVideo) {
            currentVideo.pause(); // Mevcut videoyu duraklat
        }

        // Sonraki slide'a geç
        currentSlide = (currentSlide + 1) % slides.length;

        // Yeni slide'ı göster
        slides[currentSlide].classList.add('active');
        const nextVideo = slides[currentSlide].querySelector('video');
        if (nextVideo) {
            nextVideo.currentTime = 0; // Videoyu başa sar
            nextVideo.play(); // Yeni videoyu oynat
        }
    }

    // Slider'ı başlat (her 7 saniyede bir değiştir)
    if (slides.length > 1) {
        setInterval(nextSlide, 7000);
    }

  
});
