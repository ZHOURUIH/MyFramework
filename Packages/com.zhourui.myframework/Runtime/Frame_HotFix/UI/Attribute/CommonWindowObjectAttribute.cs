using System;

// 用于标记是一个通用子页面基类,因为接口会影响所有子类也被认为是通用子页面类,所以用属性来代替
public class CommonWindowObjectAttribute : Attribute
{}