import { FirebaseApp, getApp, getApps, initializeApp } from 'firebase/app';

const firebaseConfig = {
  apiKey: 'AIzaSyCXKXxE9PyBMvBDo9C9A6qlPEKTT3vONgo',
  authDomain: 'badmintonparty-dev.firebaseapp.com',
  projectId: 'badmintonparty-dev',
  storageBucket: 'badmintonparty-dev.firebasestorage.app',
  messagingSenderId: '841167513887',
  appId: '1:841167513887:web:452ff1337003f6c45b682a',
  measurementId: 'G-KPRGTR6MM2'
};

export function initializeFirebase(): FirebaseApp {
  if (getApps().length > 0) {
    return getApp();
  }

  return initializeApp(firebaseConfig);
}
