using System;

// 用于标记是一个UI脚本基类,因为接口会影响所有子类也被认为是UI脚本基类,所以用属性来代替
public class LayoutScriptBaseAttribute : Attribute
{}