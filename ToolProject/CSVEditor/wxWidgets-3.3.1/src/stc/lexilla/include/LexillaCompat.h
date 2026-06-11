// c++14 compatibility with some c++17 features and higher.
// This helps minimize changes from the default branch.

#ifndef COMPAT_H
#define COMPAT_H

#ifdef __cplusplus

#include <cmath>
#include <cstddef>
#include <memory>
#include <type_traits>
#include <utility>

#if defined(_MSVC_LANG)
    #define wxCHECK_CXX_STD(ver) (_MSVC_LANG >= (ver))
#elif defined(__cplusplus)
    #define wxCHECK_CXX_STD(ver) (__cplusplus >= (ver))
#else
    #define wxCHECK_CXX_STD(ver) 0
#endif

#ifndef WX_HAS_CLANG_FEATURE
#   ifndef __has_feature
#       define WX_HAS_CLANG_FEATURE(x) 0
#   else
#       define WX_HAS_CLANG_FEATURE(x) __has_feature(x)
#   endif
#endif

#if defined(__GNUC__) && defined(__GNUC_MINOR__)
    #define wxCHECK_GCC_VERSION( major, minor ) ( ( __GNUC__ > (major) ) || ( __GNUC__ == (major) && __GNUC_MINOR__ >= (minor) ) )
#else
    #define wxCHECK_GCC_VERSION( major, minor ) 0
#endif

#if wxCHECK_CXX_STD(201703L)
    #define wxFALLTHROUGH [[fallthrough]]
#elif defined(__has_warning) && WX_HAS_CLANG_FEATURE(cxx_attributes)
    #define wxFALLTHROUGH [[clang::fallthrough]]
#elif wxCHECK_GCC_VERSION(7, 0)
    #define wxFALLTHROUGH __attribute__ ((fallthrough))
#endif

#ifndef wxFALLTHROUGH
    #define wxFALLTHROUGH ((void)0)
#endif

#if wxCHECK_CXX_STD(201703L)
    #define wxNODISCARD [[nodiscard]]
#elif defined(__VISUALC__)
    #define wxNODISCARD _Check_return_
#elif defined(__clang__) || defined(__GNUC__)
    #define wxNODISCARD __attribute__ ((warn_unused_result))
#else
    #define wxNODISCARD
#endif

namespace Sci {

// std::clamp
template <typename T>
inline constexpr T clamp(T val, T minVal, T maxVal) {
	return (val > maxVal) ? maxVal : ((val < minVal) ? minVal : val);
}

// std::make_unique
template<class T> struct _Unique_if {
  typedef std::unique_ptr<T> _Single_object;
};
template<class T> struct _Unique_if<T[]> {
  typedef std::unique_ptr<T[]> _Unknown_bound;
};
template<class T, size_t N> struct _Unique_if<T[N]> {
  typedef void _Known_bound;
};
template<class T, class... Args>
  typename _Unique_if<T>::_Single_object
  make_unique(Args&&... args) {
    return std::unique_ptr<T>(new T(std::forward<Args>(args)...));
  }
template<class T>
  typename _Unique_if<T>::_Unknown_bound
  make_unique(size_t n) {
    typedef typename std::remove_extent<T>::type U;
    return std::unique_ptr<T>(new U[n]());
  }
template<class T, class... Args>
  typename _Unique_if<T>::_Known_bound
  make_unique(Args&&...) = delete;

// std::size
template <typename T, size_t N>
constexpr size_t size(const T (&)[N]) noexcept {
  return N;
}

}

#endif

#endif
